using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Gommon;
using ImGuiNET;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

#nullable enable

namespace Volte.UI;

// adapted from https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/OpenGL%20Demos/ImGui/Program.cs
public sealed partial class UiManager
{
    public readonly ConcurrentQueue<Func<Task>> TaskQueue = new();

    public UiLayer[] Layers { get; }

    private int CurrentLayerIdx { get; set; }

    private void SetLayer(int layerIndex) =>
        CurrentLayerIdx = layerIndex.CoerceAtLeast(0).CoerceAtMost(Layers.Length - 1);
    
    public UiLayer CurrentLayer => Layers[CurrentLayerIdx];
    
    private UiManager(CreateParams @params)
    {
        Layers = @params.Layers;

        _window = Window.Create(@params.WOptions);

        _fontConfig = @params.Font;
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
        
        const ImGuiWindowFlags windowFlags = 
            ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar | 
            ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | 
            ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;

        var viewport = ImGui.GetMainViewport();
        
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);
        
        using (var _ = new ScopedStyleVar(ImGuiStyleVar.WindowRounding, 0f))
        using (var __ = new ScopedStyleVar(ImGuiStyleVar.WindowBorderSize, 0f))
            ImGui.Begin("Dock Space", windowFlags);
        
        ImGui.DockSpace(ImGui.GetID("DockSpace"), Vector2.Zero);

        var currentLayer = CurrentLayer;
        
        if (currentLayer.MainMenuBar is { } menuBar)
            if (ImGui.BeginMenuBar())
            {
                menuBar(delta);
                ImGui.EndMenuBar();
            }
        
        currentLayer.RenderInternal(delta);
        
        ImGui.End();

        _controller?.Render();
    }
    
    public void Dispose() => _window.Dispose();
    
    public static UiManager? Instance { get; private set; }

    public readonly struct CreateParams
    {
        public readonly WindowOptions WOptions { get; init; }
        public readonly UiLayer[] Layers { get; init; }
        public readonly ImGuiFontConfig Font { get; init; }
        public readonly Image<Rgba32>? WindowIcon { get; init; }
        public readonly string ThreadName { get; init; }
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
            
            // declared as illegal code by the Silk God (Main thread isn't the controller of the Window)
            new Thread(() =>
            {
                Instance.Run(); //returns when UI is closed
                Instance.Dispose();
                Instance = null;
            }) { Name = createParams.ThreadName }.Start();
        }
        catch (Exception e)
        {
            error = e;
            return false;
        }

        error = null;

        return true;
    }
}