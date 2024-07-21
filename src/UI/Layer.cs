using System.Collections.Concurrent;
using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL.Extensions.ImGui;

namespace Volte.UI;

public abstract class UiLayer<TState> where TState : UiLayerState
{
    public readonly ConcurrentQueue<AsyncFunction> TaskQueue = new();
    
    private readonly Dictionary<string, Action<double>> Panels = new();

    protected Func<double, bool> PreRenderCheck;

    protected Action<double> MainMenuBar;
    
    protected void Await(AsyncFunction task) => TaskQueue.Enqueue(task);
    protected void Await(Task task) => Await(() => task);

    protected void Panel(string label, Action<double> render) => Panels.Add(label, render);
    public TState State { get; protected set; }

    protected ImGuiIOPtr Io => ImGui.GetIO();

    public bool IsKeyPressed(Key key)
        => Io.KeysDown[(int)key];
    
    public bool IsMouseButtonPressed(MouseButton mb)
        => Io.MouseDown[(int)mb];

    public bool AllKeysPressed(params Key[] keys)
    {
        if (keys.Length == 1)
            return IsKeyPressed(keys[0]);
        
        var io = Io;
        return keys.Select(key => io.KeysDown[(int)key])
            .All(x => x);
    }
    
    public bool AllMouseButtonsPressed(params MouseButton[] mouseButtons)
    {
        if (mouseButtons.Length == 1)
            return Io.MouseDown[(int)mouseButtons[0]];
        
        var io = Io;
        return mouseButtons.Select(mb => io.MouseDown[(int)mb])
            .All(x => x);
    }

    protected virtual void Render(double _)
    {
    }

    internal void RenderInternal(double delta)
    {
        if (!VolteBot.IsRunning) return;

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

    private void RenderPanels(double delta)
    {
        foreach (var (panelName, renderPanel) in Panels)
        {
            ImGui.Begin(panelName);
            renderPanel(delta);
            ImGui.End();
        }
    }

    public virtual ImGuiFontConfig? GetFontConfig(int size) => null;
}

public abstract class UiLayerState
{
    public static Vector3 DefaultBackground => new(.45f, .55f, .60f);

    public Vector3 Background = DefaultBackground;
}