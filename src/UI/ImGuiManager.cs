using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

#nullable enable

namespace Volte.UI;

// adapted from https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/OpenGL%20Demos/ImGui/Program.cs
public class ImGuiManager<TState> : IDisposable where TState : ImGuiLayerState
{
    private bool _isActive = false;
    
    private readonly IWindow window;

    private ImGuiController? controller;
    private GL? gl;
    private IInputContext? inputContext;

    public ImGuiLayer<TState> Layer { get; }



    public ImGuiManager(ImGuiLayer<TState> igLayer, Gommon.Optional<WindowOptions> windowOptions = default)
    {
        Layer = igLayer;
        window = Window.Create(windowOptions.OrElse(WindowOptions.Default));

        window.Load += OnWindowLoad;
        window.Render += OnWindowRender;

        window.FramebufferResize += sz => gl.Viewport(sz);

        window.Closing += () =>
        {
            _isActive = false;
            controller?.Dispose();
            inputContext?.Dispose();
            gl?.Dispose();
        };
    }

    public void Run()
    {
        _isActive = true;
        ExecuteBackgroundAsync(async () =>
        {
            while (_isActive)
            {
                if (Layer.TaskQueue.TryDequeue(out var task))
                    await task!();
            }
        });
        window.Run();
    }
    
    private void OnWindowLoad()
    {
        controller = new ImGuiController(
            gl = window.CreateOpenGL(),
            window,
            inputContext = window.CreateInput(),
            () =>
            {
                var io = ImGui.GetIO();
                io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
                io.ConfigDockingWithShift = false;
            }
        );
        Info(LogSource.UI, $"Window 0x{window.Handle:X} loaded");
    }
    
    private void OnWindowRender(double delta)
    {
        controller?.Update((float)delta);

        gl?.ClearColor((Layer.State?.Background ?? ImGuiLayerState.DefaultBackground).AsColor());
        gl?.Clear((uint)ClearBufferMask.ColorBufferBit);

        Layer.Render(delta);

        controller?.Render();
    }

    void IDisposable.Dispose() => window.Dispose();
}