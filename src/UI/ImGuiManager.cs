using System.Numerics;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using Color = System.Drawing.Color;

namespace Volte.UI;

// adapted from https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/OpenGL%20Demos/ImGui/Program.cs
public class ImGuiManager<TState> : IDisposable where TState : ImGuiLayerState
{
    private readonly IWindow window;

    private ImGuiController controller;
    private GL gl;
    private IInputContext inputContext;

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
            controller.Dispose();
            inputContext.Dispose();
            gl.Dispose();
        };

        window.Render += delta =>
        {
            controller.Update((float)delta);

            gl.ClearColor(ImGuiLayerState.Vector3ToColor(Layer?.State?.Background ?? ImGuiLayerState.DefaultBackground));
            gl.Clear((uint)ClearBufferMask.ColorBufferBit);

            Layer?.Render(delta);

            controller.Render();
        };
    }

    public void Run() => window.Run();

    void IDisposable.Dispose() => window?.Dispose();
}

public static class ImGuiManager
{
    public static Thread CreateUiThread<TState>(ImGuiLayer<TState> layer) where TState : ImGuiLayerState
    {
        var thread = new Thread(() =>
        {
            using var manager = new ImGuiManager<TState>(layer);
            manager.Run();
        })
        {
            Name = "Volte UI Thread"
        };
        return thread;
    }
}

public abstract class ImGuiLayer<TState> where TState : ImGuiLayerState
{
    public TState State { get; protected set; }

    public abstract void Render(double delta);
}

public abstract class ImGuiLayerState
{
    public static Vector3 DefaultBackground => new(.45f, .55f, .60f);

    public Vector3 Background = DefaultBackground;

    public static Color Vector3ToColor(Vector3 vec3)
        => Color.FromArgb(255,
            (int)(Math.Min(vec3.X, 1) * 255),
            (int)(Math.Min(vec3.Y, 1) * 255),
            (int)(Math.Min(vec3.Z, 1) * 255));
}