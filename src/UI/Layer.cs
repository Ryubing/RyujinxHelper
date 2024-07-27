using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.SDL;

namespace Volte.UI;

public abstract class UiLayer
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

    public static void SetColors(ref ThemedColors theme, bool dark)
    {
        var style = ImGui.GetStyle();
        
        style.GrabRounding = 4f;
        style.FrameRounding = 6f;
        style.WindowMenuButtonPosition = ImGuiDir.None;
        style.FrameBorderSize = 1f;
        style.TabBorderSize = 1f;
        style.WindowTitleAlign = new Vector2(0.5f);
        style.SeparatorTextBorderSize = 9f;

        set(ImGuiCol.Text, theme.Gray800);
        set(ImGuiCol.TextDisabled, theme.Gray500);
        set(ImGuiCol.WindowBg, theme.Gray100);
        set(ImGuiCol.ChildBg, Spectrum.Static.None);
        set(ImGuiCol.PopupBg, theme.Gray50);
        set(ImGuiCol.Border, theme.Gray300);
        set(ImGuiCol.BorderShadow, Spectrum.Static.None);
        set(ImGuiCol.FrameBg, theme.Gray75);
        set(ImGuiCol.FrameBgHovered, theme.Gray50);
        set(ImGuiCol.FrameBgActive, theme.Gray200);
        set(ImGuiCol.TitleBg, theme.Gray300);
        set(ImGuiCol.TitleBgActive, theme.Gray200);
        set(ImGuiCol.TitleBgCollapsed, theme.Gray400);
        set(ImGuiCol.TabUnfocusedActive, theme.Blue400);
        set(ImGuiCol.MenuBarBg, theme.Gray100);
        set(ImGuiCol.ScrollbarBg, theme.Gray100);
        set(ImGuiCol.ScrollbarGrab, theme.Gray400);
        set(ImGuiCol.ScrollbarGrabHovered, theme.Gray600);
        set(ImGuiCol.ScrollbarGrabActive, theme.Gray700);
        set(ImGuiCol.CheckMark, theme.Blue500);
        set(ImGuiCol.SliderGrab, theme.Gray700);
        set(ImGuiCol.SliderGrabActive, theme.Gray800);
        set(ImGuiCol.Button, theme.Gray75);
        set(ImGuiCol.ButtonHovered, theme.Gray50);
        set(ImGuiCol.ButtonActive, theme.Gray200);
        set(ImGuiCol.Header, theme.Blue400);
        set(ImGuiCol.HeaderHovered, theme.Blue500);
        set(ImGuiCol.HeaderActive, theme.Blue600);
        set(ImGuiCol.Separator, theme.Gray400);
        set(ImGuiCol.SeparatorHovered, theme.Gray600);
        set(ImGuiCol.SeparatorActive, theme.Gray700);
        set(ImGuiCol.ResizeGrip, theme.Gray400);
        set(ImGuiCol.ResizeGripHovered, theme.Gray600);
        set(ImGuiCol.ResizeGripActive, theme.Gray700);
        set(ImGuiCol.PlotLines, theme.Blue400);
        set(ImGuiCol.PlotLinesHovered, theme.Blue600);
        set(ImGuiCol.PlotHistogram, theme.Blue400);
        set(ImGuiCol.PlotHistogramHovered, theme.Blue600);

        setVec(ImGuiCol.TextSelectedBg, ImGui.ColorConvertU32ToFloat4((colorValue(theme.Blue400) & 0x00FFFFFF) | 0x33000000));
        setVec(ImGuiCol.DragDropTarget, new Vector4(1.00f, 1.00f, 0.00f, 0.90f));
        setVec(ImGuiCol.NavHighlight, ImGui.ColorConvertU32ToFloat4((colorValue(theme.Gray900) & 0x00FFFFFF) | 0x0A000000));
        setVec(ImGuiCol.NavWindowingHighlight, new Vector4(1.00f, 1.00f, 1.00f, 0.70f));
        setVec(ImGuiCol.NavWindowingDimBg, new Vector4(0.80f, 0.80f, 0.80f, 0.20f));
        setVec(ImGuiCol.ModalWindowDimBg, new Vector4(0.20f, 0.20f, 0.20f, 0.35f));

        return;

        void set(ImGuiCol colorVar, Color color)
            => setVec(colorVar, new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, 1f));

        void setVec(ImGuiCol colorVar, Vector4 colorVec)
            => style.Colors[(int)colorVar] = colorVec;
        
        uint colorValue(Color color) => ((uint)color.R << 16)
                             | ((uint)color.G << 8)
                             | color.B;
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