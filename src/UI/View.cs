using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.SDL;

namespace Volte.UI;

public abstract class UiView
{
    private readonly List<Action<double>> _panels = [];
    
    public Action<double> MainMenuBar { get; protected set; }

    protected static ImGuiIOPtr Io => ImGui.GetIO();

    public static bool IsKeyPressed(Key key)
        => Io.KeysDown[(int)key];

    public static bool IsMouseButtonPressed(MouseButton mb)
        => Io.MouseDown[(int)mb];

    public static bool AllKeysPressed(params Key[] keys)
    {
        if (keys.Length == 1)
            return IsKeyPressed(keys[0]);

        var io = Io;
        return keys.Select(key => io.KeysDown[(int)key])
            .All(x => x);
    }

    public static bool AllMouseButtonsPressed(params MouseButton[] mouseButtons)
    {
        if (mouseButtons.Length == 1)
            return Io.MouseDown[(int)mouseButtons[0]];

        var io = Io;
        return mouseButtons.Select(mb => io.MouseDown[(int)mb])
            .All(x => x);
    }

    protected virtual bool Render(double _)
    {
        return false;
    }

    internal void RenderInternal(double delta)
    {
        if (Render(delta))
            return;
        
        foreach (var renderPanel in _panels)
            renderPanel(delta);
    }
    
    protected static void Await(Func<Task> task) => UiManager.Instance!.TaskQueue.Enqueue(task);
    protected static void Await(Task task) => Await(() => task);

    protected void Panel(string label, Action<double> render) => _panels.Add(delta =>
    {
        if (ImGui.Begin(label))
        {
            render(delta);
            ImGui.End();
        }
    });

    protected void Panel(Action<double> render) => _panels.Add(render);
    
    #region Scoped Styling

    protected IDisposable PushStyle(ImGuiStyleVar styleVar, Vector2 value) => new ScopedStyleVar(styleVar, value);
    protected IDisposable PushStyle(ImGuiStyleVar styleVar, float value) => new ScopedStyleVar(styleVar, value);
    
    protected IDisposable PushStyle(ImGuiCol colorVar, Vector4 value) => new ScopedStyleColor(colorVar, value);
    protected IDisposable PushStyle(ImGuiCol colorVar, Vector3 value) => new ScopedStyleColor(colorVar, value);
    protected IDisposable PushStyle(ImGuiCol colorVar, Color value) => new ScopedStyleColor(colorVar, value.AsVec4());
    protected IDisposable PushStyle(ImGuiCol colorVar, System.Drawing.Color value) => new ScopedStyleColor(colorVar, value.AsVec4());
    protected IDisposable PushStyle(ImGuiCol colorVar, uint value) => new ScopedStyleColor(colorVar, value);
    
    #endregion Scoped Styling
}

public struct ScopedStyleVar : IDisposable
{
    public ScopedStyleVar(ImGuiStyleVar styleVar, Vector2 value) 
        => ImGui.PushStyleVar(styleVar, value);

    public ScopedStyleVar(ImGuiStyleVar styleVar, float value) 
        => ImGui.PushStyleVar(styleVar, value);

    public void Dispose() => ImGui.PopStyleVar();
}

public struct ScopedStyleColor : IDisposable
{
    public ScopedStyleColor(ImGuiCol colorVar, Vector4 value) 
        => ImGui.PushStyleColor(colorVar, value);

    public ScopedStyleColor(ImGuiCol colorVar, Vector3 value) 
        => ImGui.PushStyleColor(colorVar, new Vector4(value, 1f));

    public ScopedStyleColor(ImGuiCol colorVar, uint value) => 
        ImGui.PushStyleColor(colorVar, value);

    public void Dispose() => ImGui.PopStyleColor();
}