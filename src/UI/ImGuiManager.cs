﻿using System.IO;
using System.Runtime.InteropServices;
using ImGuiNET;
using Silk.NET.Core;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

#nullable enable

namespace Volte.UI;

// adapted from https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/OpenGL%20Demos/ImGui/Program.cs
public sealed class ImGuiManager<TState> : IDisposable where TState : ImGuiLayerState
{
    private bool _isActive;

    private readonly IWindow _window;

    private ImGuiController? _controller;
    private GL? _gl;
    private IInputContext? _inputContext;

    public ImGuiLayer<TState> Layer { get; private set; }
    
    public ImGuiManager(ImGuiLayer<TState> igLayer, WindowOptions windowOptions)
    {
        Layer = igLayer;
        _window = Window.Create(windowOptions);

        _window.Load += OnWindowLoad;
        _window.Render += OnWindowRender;
        _window.FramebufferResize += sz => _gl?.Viewport(sz);

        _window.Closing += () =>
        {
            _isActive = false;
            _controller?.Dispose();
            _inputContext?.Dispose();
            _gl?.Dispose();
        };
    }

    public void Run()
    {
        _isActive = true;
        ExecuteBackgroundAsync(async () =>
        {
            while (_isActive) 
                if (Layer.TaskQueue.TryDequeue(out var task))
                    await task!();
        });
        _window.Run();
    }

    private void OnWindowLoad()
    {
        _gl = _window.CreateOpenGL();
        _inputContext = _window.CreateInput();

        _controller = new ImGuiController(_gl, _window, _inputContext, () =>
            {
                var io = ImGui.GetIO();
                io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
                io.ConfigDockingWithShift = false;
            }
        );
        
        // shoutout https://github.com/dotnet/Silk.NET/blob/b079b28cd51ce447183cfedde0a85412b9b226ee/src/Lab/Experiments/BlankWindow/Program.cs#L82-L95
        Stream? iconStream;
        if ((iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VolteIcon")) != null)
        {
            unsafe
            {
                var img = Image.Load<Rgba32>(iconStream);
                var memoryGroup = img.GetPixelMemoryGroup();
                
                Memory<byte> array = new byte[memoryGroup.TotalLength * sizeof(Rgba32)];
                var block = MemoryMarshal.Cast<byte, Rgba32>(array.Span);
                
                foreach (var memory in memoryGroup)
                {
                    memory.Span.CopyTo(block);
                    block = block[memory.Length..];
                }

                var rawIcon = new RawImage(img.Width, img.Height, array);
                
                img.Dispose();
                
                _window.SetWindowIcon(ref rawIcon);
            }
        }
        
        Info(LogSource.UI, $"Window 0x{_window.Handle:X} loaded");
    }

    private void OnWindowRender(double delta)
    {
        _controller?.Update((float)delta);

        _gl?.ClearColor((Layer.State?.Background ?? ImGuiLayerState.DefaultBackground).AsColor());
        _gl?.Clear((uint)ClearBufferMask.ColorBufferBit);

        Layer.RenderInternal(delta);

        _controller?.Render();
    }

    public void Dispose()
    {
        _window.Dispose();
    }
}