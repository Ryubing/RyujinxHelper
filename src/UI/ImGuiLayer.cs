using System.Collections.Concurrent;
using System.Numerics;
using ImGuiNET;

namespace Volte.UI;

public abstract class ImGuiLayer<TState> where TState : ImGuiLayerState
{
    public readonly ConcurrentQueue<AsyncFunction> TaskQueue = new();
    
    protected void Await(AsyncFunction task) => TaskQueue.Enqueue(task);
    protected void Await(Task task) => Await(() => task);
    
    public TState State { get; protected set; }

    public ImGuiIOPtr Io => ImGui.GetIO();

    public abstract void Render(double delta);
}

public abstract class ImGuiLayerState
{
    public static Vector3 DefaultBackground => new(.45f, .55f, .60f);

    public Vector3 Background = DefaultBackground;
}