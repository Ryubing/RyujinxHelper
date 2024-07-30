using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.SDL;

namespace Volte.UI;

[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")] //this is an API
public abstract class UiView
{
    private readonly List<Action<double>> _panels = [];
    
    public Action<double> MainMenuBar { get; protected init; }

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

    /**
     * Override this function for custom one-off rendering.
     * The return value determines whether your <see cref="UiView"/>'s defined panels will be rendered.
     * (false = no render, true = render)
     */
    protected virtual bool Render(double _) => true;

    internal void RenderInternal(double delta)
    {
        if (!Render(delta)) return;
        
        foreach (var renderPanel in _panels)
            renderPanel(delta);
    }
    
    protected static void Await(Func<Task> task) => Await(task());
    protected static void Await(Task task) => UiManager.Instance!.TaskQueue.Enqueue(task);

    protected void Panel(string label, Action<double> render) => _panels.Add(delta =>
    {
        if (ImGui.Begin(label))
        {
            render(delta);
            ImGui.End();
        }
    });

    protected void Panel(Action<double> render) => _panels.Add(render);
}