using System.Collections.Concurrent;
using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;

namespace Volte.UI;

public abstract class ImGuiLayer<TState> where TState : ImGuiLayerState
{
    public readonly ConcurrentQueue<AsyncFunction> TaskQueue = new();
    
    protected readonly Dictionary<string, Action<double>> Panels = new();

    protected Func<double, bool> PreRenderCheck;

    protected Action<double> MainMenuBar;
    
    protected void Await(AsyncFunction task) => TaskQueue.Enqueue(task);
    protected void Await(Task task) => Await(() => task);
    
    public TState State { get; protected set; }

    public ImGuiIOPtr Io => ImGui.GetIO();

    public bool IsKeyPressed(Key key)
        => Io.KeysDown[(int)key];
    
    public bool IsMouseButtonPressed(MouseButton mb)
        => Io.MouseDown[(int)mb];

    public bool AllKeysPressed(params Key[] keys)
    {
        var io = Io;
        return keys.Select(key => io.KeysDown[(int)key])
            .All(x => x);
    }
    
    public bool AllMouseButtonsPressed(params MouseButton[] keys)
    {
        var io = Io;
        return keys.Select(key => io.MouseDown[(int)key])
            .All(x => x);
    }

    public virtual void Render(double delta)
    {
        
    }

    public void RenderInternal(double delta)
    {
        if (!PreRenderCheck?.Invoke(delta) ?? false) return;

        if (MainMenuBar is not null)
        {
            if (ImGui.BeginMainMenuBar())
            {
                MainMenuBar(delta);
                ImGui.EndMainMenuBar();
            }
        }
        
        Render(delta);
        RenderPanels(delta);
    }

    protected void RenderPanels(double delta)
    {
        foreach (var (panelName, renderPanel) in Panels)
        {
            ImGui.Begin(panelName);
            renderPanel(delta);
            ImGui.End();
        }
    }
}

public abstract class ImGuiLayerState
{
    public static Vector3 DefaultBackground => new(.45f, .55f, .60f);

    public Vector3 Background = DefaultBackground;
}