using System.Collections.Concurrent;
using System.Numerics;

namespace Volte.UI;

public abstract class ImGuiLayer<TState> where TState : ImGuiLayerState
{
    public readonly ConcurrentQueue<Func<Task>> TaskQueue = new();
    
    public TState State { get; protected set; }

    public abstract void Render(double delta);
}

public abstract class ImGuiLayerState
{
    public static Vector3 DefaultBackground => new(.45f, .55f, .60f);

    public Vector3 Background = DefaultBackground;
}