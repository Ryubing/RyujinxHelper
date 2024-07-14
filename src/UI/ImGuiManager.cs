using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

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
        Info(LogSource.UI, $"Window 0x{_window.Handle:X} loaded");
    }

    private void OnWindowRender(double delta)
    {
        _controller?.Update((float)delta);

        _gl?.ClearColor((Layer.State?.Background ?? ImGuiLayerState.DefaultBackground).AsColor());
        _gl?.Clear((uint)ClearBufferMask.ColorBufferBit);

        Layer.Render(delta);

        _controller?.Render();
    }

    public void Dispose()
    {
        _window.Dispose();
        VolteBot.ImGui = null;
    }
}