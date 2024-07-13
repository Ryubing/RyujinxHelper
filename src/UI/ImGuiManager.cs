using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

#nullable enable

namespace Volte.UI;

// adapted from https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/OpenGL%20Demos/ImGui/Program.cs
public class ImGuiManager<TState> : IDisposable where TState : ImGuiLayerState
{
    private readonly IWindow window;

    private ImGuiController? controller;
    private GL? gl;
    private IInputContext? inputContext;

    public ImGuiLayer<TState> Layer { get; }

    public ImGuiManager(ImGuiLayer<TState> igLayer, Gommon.Optional<WindowOptions> windowOptions = default)
    {
        Layer = igLayer;
        window = Window.Create(windowOptions.OrElse(WindowOptions.Default));

        window.Load += () =>
        {
            controller = new ImGuiController(
                gl = window.CreateOpenGL(),
                window,
                inputContext = window.CreateInput()
            );
            Info(LogSource.UI, $"Window 0x{window.Handle:X} loaded");
        };

        window.FramebufferResize += sz => gl.Viewport(sz);

        window.Closing += () =>
        {
            controller?.Dispose();
            inputContext?.Dispose();
            gl?.Dispose();
        };

        window.Render += delta =>
        {
            controller?.Update((float)delta);

            gl?.ClearColor((Layer.State?.Background ?? ImGuiLayerState.DefaultBackground).AsColor());
            gl?.Clear((uint)ClearBufferMask.ColorBufferBit);

            Layer.Render(delta);

            controller?.Render();
        };
    }

    public void Run()
    {
        ExecuteBackgroundAsync(async () =>
        {
            while (true)
            {
                if (Layer.TaskQueue.TryDequeue(out var task))
                    await task!();
            }
        });
        window.Run();
    }

    void IDisposable.Dispose() => window.Dispose();
}