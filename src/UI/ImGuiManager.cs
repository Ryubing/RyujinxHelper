using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace Volte.UI;

public class ImGuiManager : IDisposable
{
    private readonly IWindow window;

    private ImGuiController controller;
    private GL gl;
    private IInputContext inputContext;
    
    public ImGuiLayer Layer { get; }

    public ImGuiManager(ImGuiLayer igLayer, Gommon.Optional<WindowOptions> windowOptions = default)
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

            gl.ClearColor(System.Drawing.Color.FromArgb(255, (int)(.45f * 255), (int)(.55f * 255), (int)(.60f * 255)));
            gl.Clear((uint)ClearBufferMask.ColorBufferBit);

            Layer?.Render(delta);
            
            controller.Render();
        };
    }

    public static Thread CreateUiThread(ImGuiLayer layer)
    {
        var thread = new Thread(() =>
        {
            using var manager = new ImGuiManager(layer);
            manager.Run();
        })
        {
            Name = "Volte UI Thread"
        };
        return thread;
    }

    public void Run() => window.Run();

    void IDisposable.Dispose() => window?.Dispose();
}

public abstract class ImGuiLayer
{
    public abstract void Render(double delta);
}