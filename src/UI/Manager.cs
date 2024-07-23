using System.IO;
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
    private readonly ImGuiFontConfig? _fontConfig;

    private ImGuiController? _controller;
    private GL? _gl;
    private IInputContext? _inputContext;

    public UiLayer<TState> Layer { get; }

    public UiManager(UiLayer<TState> igLayer, WindowOptions? windowOptions, int fontSize = 14)
    {
        Layer = igLayer;

        _window = Window.Create(windowOptions ?? WindowOptions.Default);

        _fontConfig = Layer.GetFontConfig(fontSize);

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
        _gl = GL.GetApi(_window);
        _inputContext = _window.CreateInput();
        _controller = new ImGuiController(_gl, _window, _inputContext, _fontConfig, () =>
            {
                var io = ImGui.GetIO();
                io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
                io.ConfigDockingWithShift = false;

                Layer.SetColors(ref Spectrum.Dark);

                var fonts = FilePath.Data.Resolve("fonts", true);
                if (fonts.ExistsAsDirectory)
                    fonts.GetFiles()
                        .Where(x => x.Extension is "ttf")
                        .ForEach(fp => io.Fonts.AddFontFromFileTTF(fp.Path, 17));
            }
        );

        SetWindowIcon();

        Info(LogSource.UI, $"Window 0x{_window.Handle:X} loaded");
    }

    private void OnWindowRender(double delta)
    {
        if (_window.WindowState == WindowState.Minimized) return;

        _controller?.Update((float)delta);

        Layer.RenderInternal(delta);

        _controller?.Render();
    }

    private void SetWindowIcon()
    {
        // shoutout https://github.com/dotnet/Silk.NET/blob/b079b28cd51ce447183cfedde0a85412b9b226ee/src/Lab/Experiments/BlankWindow/Program.cs#L82-L95
        Stream? iconStream;
        if ((iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VolteIcon")) == null) return;

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

    public void Dispose() => _window.Dispose();
}

public static class UiManager
{
    public static UiManager<VolteUiState>? Instance { get; private set; }

    public static bool TryCreateUi(
        IServiceProvider provider, WindowOptions? windowOptions,
        int fontSize, out string error
    )
    {
        if (Instance is not null)
        {
            error = "UI is already open.";
            return false;
        }

        try
        {
            Instance = Create(new VolteUiLayer(provider), windowOptions ?? VolteBot.DefaultWindowOptions, fontSize);
            
            // declared as illegal code by the Silk God (Main thread isn't the controller of the Window)
            new Thread(() =>
            {
                Instance.Run(); //returns when UI is closed
                Instance.Dispose();
                Instance = null;
            }) { Name = "Volte UI Thread" }.Start();
        }
        catch (Exception e)
        {
            Error(LogSource.UI, "Could not create UI thread", e);
            error = $"Error opening UI: {e.Message}";
            return false;
        }

        error = null;

        return true;
    }

    public static UiManager<TState> Create<TState>(UiLayer<TState> layer, WindowOptions? windowOptions = null,
        int fontSize = 14) where TState : UiLayerState
        => new(layer, windowOptions, fontSize);
}