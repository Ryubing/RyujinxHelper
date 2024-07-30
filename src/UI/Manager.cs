using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    public readonly ConcurrentQueue<Func<Task>> TaskQueue = new();

    public readonly List<UiView> Views = [];

    public unsafe ThemedColors* Theme { get; }

    private int CurrentViewIdx { get; set; }

    private void SetView(int viewIndex) =>
        CurrentViewIdx = viewIndex.CoerceAtLeast(0).CoerceAtMost(Views.Count - 1);
    
    public UiView CurrentView => Views[CurrentViewIdx];
    
    private UiManager(CreateParams @params)
    {
        unsafe
        {
            Theme = @params.Theme;
        }

        _window = Window.Create(@params.WOptions);

        _onConfigureIO = @params.OnConfigureIO;
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
        Executor.ExecuteBackgroundAsync(async () =>
        {
            while (_isActive)
                if (TaskQueue.TryDequeue(out var task))
                    await task();
            
            TaskQueue.Clear();
        });
        _window.Run();
    }

    private void OnWindowRender(double delta)
    {
        if (_window.WindowState == WindowState.Minimized) return;

        _controller?.Update((float)delta);
        
        // shoutout https://gist.github.com/moebiussurfing/8dbc7fef5964adcd29428943b78e45d2
        // for showing me how to properly setup dock space
        
        var windowFlags = 
            ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar | 
            ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | 
            ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;

        var currView = CurrentView;
        if (currView.MainMenuBar != null)
            windowFlags |= ImGuiWindowFlags.MenuBar;
        
        var viewport = ImGui.GetMainViewport();
        
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);
        
        using (new ScopedStyleVar(ImGuiStyleVar.WindowRounding, 0f))
        using (new ScopedStyleVar(ImGuiStyleVar.WindowBorderSize, 0f))
            ImGui.Begin("Dock Space", windowFlags);
        
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
        public Action<ImGuiIOPtr> OnConfigureIO { get; init; }
        public Image<Rgba32>? WindowIcon { get; init; }
        
        public unsafe ThemedColors* Theme { get; init; }
        
        public string ThreadName { get; init; }
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
    
    public static void AddView(UiView view) => Instance!.Views.Add(view);
}