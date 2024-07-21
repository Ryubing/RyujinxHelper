using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;
using Silk.NET.Core;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

#nullable enable

namespace Volte.UI;

// adapted from https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/OpenGL%20Demos/ImGui/Program.cs
public sealed class UiManager<TState> : IDisposable where TState : UiLayerState
{
    private bool _isActive;

    private readonly IWindow _window;

    private ImGuiController? _controller;
    private GL? _gl;
    private IInputContext? _inputContext;

    private readonly ImGuiFontConfig? _fontConfig;

    public UiLayer<TState> Layer { get; }

    public UiManager(UiLayer<TState> igLayer, WindowOptions? windowOptions, int fontSize = 14)
    {
        Layer = igLayer;
        _window = Window.Create(windowOptions ?? WindowOptions.Default);

        _window.Load += OnWindowLoad;
        _window.Render += OnWindowRender;
        _window.FramebufferResize += sz => _gl?.Viewport(sz);

        _window.Closing += () =>
        {
            _isActive = false;
            _controller?.Dispose();
            _inputContext?.Dispose();
            _gl?.Dispose();
        };

        _fontConfig = Layer.GetFontConfig(fontSize);
    }

    public void Run()
    {
        _isActive = true;
        ExecuteBackgroundAsync(async () =>
        {
            while (_isActive)
                if (Layer.TaskQueue.TryDequeue(out var task))
                    await task!();
        });
        _window.Run();
    }

    private void OnWindowLoad()
    {
        _gl = _window.CreateOpenGL();
        _inputContext = _window.CreateInput();
        _controller = new ImGuiController(_gl, _window, _inputContext, _fontConfig, () =>
            {
                var io = ImGui.GetIO();
                io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
                io.ConfigDockingWithShift = false;
                SetColors();
            }
        );

        // shoutout https://github.com/dotnet/Silk.NET/blob/b079b28cd51ce447183cfedde0a85412b9b226ee/src/Lab/Experiments/BlankWindow/Program.cs#L82-L95
        Stream? iconStream;
        if ((iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VolteIcon")) != null)
        {
            var img = Image.Load<Rgba32>(iconStream);
            var memoryGroup = img.GetPixelMemoryGroup();

            Memory<byte> array = new byte[memoryGroup.TotalLength * Unsafe.SizeOf<Rgba32>()];
            var block = MemoryMarshal.Cast<byte, Rgba32>(array.Span);

            foreach (var memory in memoryGroup)
            {
                memory.Span.CopyTo(block);
                block = block[memory.Length..];
            }

            var rawIcon = new RawImage(img.Width, img.Height, array);

            img.Dispose();

            _window.SetWindowIcon(ref rawIcon);
        }

        Info(LogSource.UI, $"Window 0x{_window.Handle:X} loaded");
    }

    private void OnWindowRender(double delta)
    {
        _controller?.Update((float)delta);

        _gl?.ClearColor((Layer.State?.Background ?? UiLayerState.DefaultBackground).AsColor());
        _gl?.Clear((uint)ClearBufferMask.ColorBufferBit);

        Layer.RenderInternal(delta);

        _controller?.Render();
    }

    private static void SetColors()
    {
        var style = ImGui.GetStyle();
        style.GrabRounding = 4f;

        set(ImGuiCol.Text, Spectrum.Gray800);
        set(ImGuiCol.TextDisabled, Spectrum.Gray500);
        set(ImGuiCol.WindowBg, Spectrum.Gray100);
        set(ImGuiCol.ChildBg, Spectrum.Static.None);
        set(ImGuiCol.PopupBg, Spectrum.Gray50);
        set(ImGuiCol.Border, Spectrum.Gray300);
        set(ImGuiCol.BorderShadow, Spectrum.Static.None);
        set(ImGuiCol.FrameBg, Spectrum.Gray75);
        set(ImGuiCol.FrameBgHovered, Spectrum.Gray50);
        set(ImGuiCol.FrameBgActive, Spectrum.Gray200);
        set(ImGuiCol.TitleBg, Spectrum.Gray300);
        set(ImGuiCol.TitleBgActive, Spectrum.Gray200);
        set(ImGuiCol.TitleBgCollapsed, Spectrum.Gray400);
        set(ImGuiCol.MenuBarBg, Spectrum.Gray100);
        set(ImGuiCol.ScrollbarBg, Spectrum.Gray100);
        set(ImGuiCol.ScrollbarGrab, Spectrum.Gray400);
        set(ImGuiCol.ScrollbarGrabHovered, Spectrum.Gray600);
        set(ImGuiCol.ScrollbarGrabActive, Spectrum.Gray700);
        set(ImGuiCol.CheckMark, Spectrum.Blue500);
        set(ImGuiCol.SliderGrab, Spectrum.Gray700);
        set(ImGuiCol.SliderGrabActive, Spectrum.Gray800);
        set(ImGuiCol.Button, Spectrum.Gray75);
        set(ImGuiCol.ButtonHovered, Spectrum.Gray50);
        set(ImGuiCol.ButtonActive, Spectrum.Gray200);
        set(ImGuiCol.Header, Spectrum.Blue400);
        set(ImGuiCol.HeaderHovered, Spectrum.Blue500);
        set(ImGuiCol.HeaderActive, Spectrum.Blue600);
        set(ImGuiCol.Separator, Spectrum.Gray400);
        set(ImGuiCol.SeparatorHovered, Spectrum.Gray600);
        set(ImGuiCol.SeparatorActive, Spectrum.Gray700);
        set(ImGuiCol.ResizeGrip, Spectrum.Gray400);
        set(ImGuiCol.ResizeGripHovered, Spectrum.Gray600);
        set(ImGuiCol.ResizeGripActive, Spectrum.Gray700);
        set(ImGuiCol.PlotLines, Spectrum.Blue400);
        set(ImGuiCol.PlotLinesHovered, Spectrum.Blue600);
        set(ImGuiCol.PlotHistogram, Spectrum.Blue400);
        set(ImGuiCol.PlotHistogramHovered, Spectrum.Blue600);
        
        setVec(ImGuiCol.TextSelectedBg, ImGui.ColorConvertU32ToFloat4((Spectrum.Blue400 & 0x00FFFFFF) | 0x33000000));
        setVec(ImGuiCol.DragDropTarget, new Vector4(1.00f, 1.00f, 0.00f, 0.90f));
        setVec(ImGuiCol.NavHighlight, ImGui.ColorConvertU32ToFloat4((Spectrum.Gray900 & 0x00FFFFFF) | 0x0A000000));
        setVec(ImGuiCol.NavWindowingHighlight, new Vector4(1.00f, 1.00f, 1.00f, 0.70f));
        setVec(ImGuiCol.NavWindowingDimBg, new Vector4(0.80f, 0.80f, 0.80f, 0.20f));
        setVec(ImGuiCol.ModalWindowDimBg, new Vector4(0.20f, 0.20f, 0.20f, 0.35f));

        return;
        
        void set(ImGuiCol colorVar, Color color)
            => style.Colors[(int)colorVar] = color.AsVec4();

        void setVec(ImGuiCol colorVar, Vector4 colorVec)
            => style.Colors[(int)colorVar] = colorVec;
    }


    public void Dispose() => _window.Dispose();
}

public static class UiManager
{
    public static UiManager<TState> Create<TState>(UiLayer<TState> layer, WindowOptions? windowOptions = null,
        int fontSize = 14) where TState : UiLayerState
        => new(layer, windowOptions, fontSize);
}