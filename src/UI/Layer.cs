using System.Collections.Concurrent;
using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL.Extensions.ImGui;

namespace Volte.UI;

public abstract class UiLayer<TState> where TState : UiLayerState
{
    public readonly ConcurrentQueue<AsyncFunction> TaskQueue = new();

    private readonly List<Action<double>> Panels = [];

    protected Func<double, bool> PreRenderCheck;

    protected Action<double> MainMenuBar;

    protected void Await(AsyncFunction task) => TaskQueue.Enqueue(task);
    protected void Await(Task task) => Await(() => task);

    protected void Panel(string label, Action<double> render) => Panels.Add(delta =>
    {
        if (ImGui.Begin(label))
        {
            render(delta);
            ImGui.End();
        }
    });

    protected void Panel(Action<double> render) => Panels.Add(render);
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
        
        // shoutout https://gist.github.com/moebiussurfing/8dbc7fef5964adcd29428943b78e45d2
        // for showing me how to properly setup dock space
        
        const ImGuiWindowFlags windowFlags = 
            ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar | 
            ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | 
            ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;

        var viewport = ImGui.GetMainViewport();
        
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);
        
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, State.Background.AsColor().AsVec4());

        ImGui.Begin("Dock Space", windowFlags);
        
        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor(1);

        var dockspaceId = ImGui.GetID("DockSpace");
        ImGui.DockSpace(dockspaceId, Vector2.Zero);
        
        if (MainMenuBar is not null)
        {
            if (ImGui.BeginMenuBar())
            {
                MainMenuBar(delta);
                ImGui.EndMenuBar();
            }
        }

        Render(delta);
        RenderPanels(delta);
        
        ImGui.End();
    }

    private void RenderPanels(double delta)
    {
        foreach (var renderPanel in Panels)
            renderPanel(delta);
    }

    public void SetColors(ref ThemedColors theme)
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

        setVec(ImGuiCol.TextSelectedBg, ImGui.ColorConvertU32ToFloat4((theme.Blue400 & 0x00FFFFFF) | 0x33000000));
        setVec(ImGuiCol.DragDropTarget, new Vector4(1.00f, 1.00f, 0.00f, 0.90f));
        setVec(ImGuiCol.NavHighlight, ImGui.ColorConvertU32ToFloat4((theme.Gray900 & 0x00FFFFFF) | 0x0A000000));
        setVec(ImGuiCol.NavWindowingHighlight, new Vector4(1.00f, 1.00f, 1.00f, 0.70f));
        setVec(ImGuiCol.NavWindowingDimBg, new Vector4(0.80f, 0.80f, 0.80f, 0.20f));
        setVec(ImGuiCol.ModalWindowDimBg, new Vector4(0.20f, 0.20f, 0.20f, 0.35f));

        return;

        void set(ImGuiCol colorVar, Color color)
            => style.Colors[(int)colorVar] = color.AsVec4();

        void setVec(ImGuiCol colorVar, Vector4 colorVec)
            => style.Colors[(int)colorVar] = colorVec;
    }

    public virtual ImGuiFontConfig? GetFontConfig(int size) => null;
}

public abstract class UiLayerState
{
    public static Vector3 DefaultBackground => new(.45f, .55f, .60f);

    public Vector3 Background = DefaultBackground;

    public bool SelectedTheme { get; set; } = true;
}