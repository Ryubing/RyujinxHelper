﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Gommon;
using ImGuiNET;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

#nullable enable

namespace Volte.UI;

// adapted from https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/OpenGL%20Demos/ImGui/Program.cs
public sealed partial class UiManager
{
    public readonly ConcurrentQueue<Task> TaskQueue = new();

    private readonly List<UiView> _views = [];

    private readonly unsafe ThemedColors* _theme;

    private int CurrentViewIdx { get; set; }

    private void SetView(int viewIndex) =>
        CurrentViewIdx = viewIndex.CoerceAtLeast(0).CoerceAtMost(_views.Count - 1);

    public UiView CurrentView => _views[CurrentViewIdx];

    private unsafe UiManager(CreateParams @params)
    {
        _theme = @params.Theme;

        _window = Window.Create(@params.WOptions);

        _onConfigureIO = @params.OnConfigureIo;
        _windowIcon = @params.WindowIcon;

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
    }

    public void Run()
    {
        _isActive = true;

        Lambda
            .Repeat(async () =>
            {
                if (TaskQueue.TryDequeue(out var task))
                    await task.ConfigureAwait(false);
            })
            .While(() => _isActive)
            .Finally(() => TaskQueue.Clear())
            .Async();
        
        _window.Run();
    }

    private const ImGuiWindowFlags DockSpaceFlags =
        ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar |
        ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
        ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;

    private void OnWindowRender(double delta)
    {
        if (_window.WindowState == WindowState.Minimized) return;

        _controller?.Update((float)delta);

        // shoutout https://gist.github.com/moebiussurfing/8dbc7fef5964adcd29428943b78e45d2
        // for showing me how to properly setup dock space

        var currView = CurrentView;

        var viewport = ImGui.GetMainViewport();

        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);

        using (ImGuiStyleVar.WindowRounding.PushValue(0f))
        using (ImGuiStyleVar.WindowBorderSize.PushValue(0f))
            ImGui.Begin("Dock Space",
                currView.MainMenuBar != null
                    ? DockSpaceFlags | ImGuiWindowFlags.MenuBar
                    : DockSpaceFlags
            );

        ImGui.DockSpace(ImGui.GetID("DockSpace"), Vector2.Zero);

        if (currView.MainMenuBar is { } menuBar)
            if (ImGui.BeginMenuBar())
            {
                menuBar(delta);
                ImGui.EndMenuBar();
            }

        currView.RenderInternal(delta);

        ImGui.End();

        _controller?.Render();
    }

    public void Dispose() => _window.Dispose();

    public static UiManager? Instance { get; private set; }

    public readonly struct CreateParams
    {
        public WindowOptions WOptions { get; init; }
        public Action<ImGuiIOPtr> OnConfigureIo { get; init; }
        public Image<Rgba32>? WindowIcon { get; init; }
        public unsafe ThemedColors* Theme { get; init; }
    }

    public static bool TryCreateUi(CreateParams createParams, out Exception? error)
    {
        if (Instance is not null)
        {
            error = new InvalidOperationException("UI is already open.");
            return false;
        }

        try
        {
            Instance = new UiManager(createParams);
        }
        catch (Exception e)
        {
            error = e;
            return false;
        }

        error = null;

        return true;
    }

    public static void StartThread(string threadName)
    {
        // declared as illegal code by the Silk God (Main thread isn't the controller of the Window)
        new Thread(() =>
        {
            Instance?.Run(); //returns when UI is closed
            Instance?.Dispose();
            Instance = null;
        }) { Name = threadName }.Start();
    }

    public static void AddView(UiView view) => Instance!._views.Add(view);

    public static void LoadFontFromStream(Stream stream, string fontName, float fontSize)
    {
        stream.Seek(0, SeekOrigin.Begin);

        var fontData = new byte[stream.Length];
        
        // no using is a deliberate choice here; the user is responsible for disposing the stream
        var read = new BinaryReader(stream).Read(fontData, 0, fontData.Length);
        if (read != fontData.Length)
            throw new InvalidDataException("Could not read all bytes from the stream.");


        unsafe
        {
            var fontConfig = ImGuiNative.ImFontConfig_ImFontConfig();
            fontName
                .Select(c => (byte)c)
                .Take(40)
                .ToArray()
                .CopyTo(new Span<byte>(fontConfig->Name, 40));

            fixed (byte* fontDataPtr = fontData)
                ImGui.GetIO().Fonts.AddFontFromMemoryTTF((nint)fontDataPtr, fontData.Length, fontSize,
                    new ImFontConfigPtr(fontConfig));
        }
    }
}