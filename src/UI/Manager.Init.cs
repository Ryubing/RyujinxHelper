using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gommon;
using ImGuiNET;
using Silk.NET.Core;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace Volte.UI;

#nullable enable

public sealed partial class UiManager : IDisposable
{
    private bool _isActive;

    private readonly IWindow _window;
    private readonly ImGuiFontConfig? _fontConfig;
    private Image<Rgba32>? _windowIcon;

    private ImGuiController? _controller;
    private GL? _gl;
    private IInputContext? _inputContext;
    
    private void OnWindowLoad()
    {
        _gl = GL.GetApi(_window);
        _inputContext = _window.CreateInput();
        _controller = new ImGuiController(_gl, _window, _inputContext, _fontConfig, () =>
            {
                var io = ImGui.GetIO();
                io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
                io.ConfigDockingWithShift = false;

                UiLayer.SetColors(ref Spectrum.Dark, true);

                var fonts = FilePath.Data.Resolve("fonts", true);
                if (fonts.ExistsAsDirectory)
                    fonts.GetFiles()
                        .Where(x => x.Extension is "ttf")
                        .ForEach(fp => io.Fonts.AddFontFromFileTTF(fp.Path, 17));
            }
        );

        SetWindowIcon();
    }

    private void SetWindowIcon()
    {
        // shoutout https://github.com/dotnet/Silk.NET/blob/b079b28cd51ce447183cfedde0a85412b9b226ee/src/Lab/Experiments/BlankWindow/Program.cs#L82-L95

        if (_windowIcon == null) return;
        
        Memory<byte> array = new byte[_windowIcon.GetPixelMemoryGroup().TotalLength * Unsafe.SizeOf<Rgba32>()];
        var block = MemoryMarshal.Cast<byte, Rgba32>(array.Span);

        foreach (var memory in _windowIcon.GetPixelMemoryGroup())
        {
            memory.Span.CopyTo(block);
            block = block[memory.Length..];
        }

        var rawIcon = new RawImage(_windowIcon.Width, _windowIcon.Height, array);

        _windowIcon.Dispose();
        _windowIcon = null;

        _window.SetWindowIcon(ref rawIcon);
    }
}