using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gommon;
using ImGuiNET;
using Silk.NET.Core;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Color = Silk.NET.SDL.Color;

namespace Volte.UI;

#nullable enable

public sealed partial class UiManager : IDisposable
{
    private bool _isActive;

    private readonly IWindow _window;
    private readonly Action<ImGuiIOPtr> _onConfigureIO;
    private Image<Rgba32>? _windowIcon;

    private ImGuiController? _controller;
    private GL? _gl;
    private IInputContext? _inputContext;
    
    private void OnWindowLoad()
    {
        _gl = GL.GetApi(_window);
        _inputContext = _window.CreateInput();
        _controller = new ImGuiController(_gl, _window, _inputContext, onConfigureIO: () =>
            {
                var io = ImGui.GetIO();
                io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
                io.ConfigDockingWithShift = false;

                unsafe
                {
                    if (_theme != null)
                        SetColors(_theme);
                    else 
                        ImGui.StyleColorsDark();
                }

                _onConfigureIO(io);
            }
        );

        SetWindowIcon();
    }

    private void SetWindowIcon()
    {
        // shoutout https://github.com/dotnet/Silk.NET/blob/b079b28cd51ce447183cfedde0a85412b9b226ee/src/Lab/Experiments/BlankWindow/Program.cs#L82-L95

        if (_windowIcon == null) return;
        
        Memory<byte> array = new byte[_windowIcon.GetPixelMemoryGroup().TotalLength * Unsafe.SizeOf<Rgba32>()];
        var block = MemoryMarshal.Cast<byte, Rgba32>(array.Span);

        foreach (var memory in _windowIcon.GetPixelMemoryGroup())
        {
            memory.Span.CopyTo(block);
            block = block[memory.Length..];
        }

        var rawIcon = new RawImage(_windowIcon.Width, _windowIcon.Height, array);

        _windowIcon.Dispose();
        _windowIcon = null;

        _window.SetWindowIcon(ref rawIcon);
    }
    
    public static unsafe void SetColors(ThemedColors* theme)
    {
        var style = ImGui.GetStyle();
        
        style.GrabRounding = 4f;
        style.FrameRounding = 6f;
        style.WindowMenuButtonPosition = ImGuiDir.None;
        style.FrameBorderSize = 1f;
        style.TabBorderSize = 1f;
        style.WindowTitleAlign = new Vector2(0.5f);
        style.SeparatorTextBorderSize = 9f;

        set(ImGuiCol.Text, theme->Gray800);
        set(ImGuiCol.TextDisabled, theme->Gray500);
        set(ImGuiCol.WindowBg, theme->Gray100);
        set(ImGuiCol.ChildBg, Spectrum.Static.None);
        set(ImGuiCol.PopupBg, theme->Gray50);
        set(ImGuiCol.Border, theme->Gray300);
        set(ImGuiCol.BorderShadow, Spectrum.Static.None);
        set(ImGuiCol.FrameBg, theme->Gray75);
        set(ImGuiCol.FrameBgHovered, theme->Gray50);
        set(ImGuiCol.FrameBgActive, theme->Gray200);
        set(ImGuiCol.TitleBg, theme->Gray300);
        set(ImGuiCol.TitleBgActive, theme->Gray200);
        set(ImGuiCol.TitleBgCollapsed, theme->Gray400);
        set(ImGuiCol.TabUnfocusedActive, theme->Blue400);
        set(ImGuiCol.MenuBarBg, theme->Gray100);
        set(ImGuiCol.ScrollbarBg, theme->Gray100);
        set(ImGuiCol.ScrollbarGrab, theme->Gray400);
        set(ImGuiCol.ScrollbarGrabHovered, theme->Gray600);
        set(ImGuiCol.ScrollbarGrabActive, theme->Gray700);
        set(ImGuiCol.CheckMark, theme->Blue500);
        set(ImGuiCol.SliderGrab, theme->Gray700);
        set(ImGuiCol.SliderGrabActive, theme->Gray800);
        set(ImGuiCol.Button, theme->Gray75);
        set(ImGuiCol.ButtonHovered, theme->Gray50);
        set(ImGuiCol.ButtonActive, theme->Gray200);
        set(ImGuiCol.Header, theme->Blue400);
        set(ImGuiCol.HeaderHovered, theme->Blue500);
        set(ImGuiCol.HeaderActive, theme->Blue600);
        set(ImGuiCol.Separator, theme->Gray400);
        set(ImGuiCol.SeparatorHovered, theme->Gray600);
        set(ImGuiCol.SeparatorActive, theme->Gray700);
        set(ImGuiCol.ResizeGrip, theme->Gray400);
        set(ImGuiCol.ResizeGripHovered, theme->Gray600);
        set(ImGuiCol.ResizeGripActive, theme->Gray700);
        set(ImGuiCol.PlotLines, theme->Blue400);
        set(ImGuiCol.PlotLinesHovered, theme->Blue600);
        set(ImGuiCol.PlotHistogram, theme->Blue400);
        set(ImGuiCol.PlotHistogramHovered, theme->Blue600);

        setVec(ImGuiCol.TextSelectedBg, ImGui.ColorConvertU32ToFloat4((colorValue(theme->Blue400) & 0x00FFFFFF) | 0x33000000));
        setVec(ImGuiCol.DragDropTarget, new Vector4(1.00f, 1.00f, 0.00f, 0.90f));
        setVec(ImGuiCol.NavHighlight, ImGui.ColorConvertU32ToFloat4((colorValue(theme->Gray900) & 0x00FFFFFF) | 0x0A000000));
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
}