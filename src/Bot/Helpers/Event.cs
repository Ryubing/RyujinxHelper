using System.Collections.Immutable;

namespace Volte.Helpers;

internal class Event<T>
    where T : class
{
    private readonly object _subLock = new();
    private ImmutableArray<T> _subscriptions = [];

    public bool HasSubscribers
    {
        get
        {
            lock (_subLock)
                return _subscriptions.Length != 0;
        }
    }

    public IReadOnlyList<T> Subscriptions
    {
        get
        {
            lock (_subLock)
                return _subscriptions;
        }
    }

    public void Add(T subscriber)
    {
        Guard.Require(subscriber, nameof(subscriber));
        lock (_subLock)
            _subscriptions = _subscriptions.Add(subscriber);
    }

    public void Remove(T subscriber)
    {
        Guard.Require(subscriber, nameof(subscriber));
        lock (_subLock)
            _subscriptions = _subscriptions.Remove(subscriber);
    }

    public void Clear()
    {
        lock (_subLock)
            _subscriptions = [];
    }
}

internal static class EventExtensions
{
    public static Task CallAsync(this Event<Func<Task>> eventHandler)
        => eventHandler.Subscriptions.ForEachAsync(x => x());

    public static Task CallAsync<T>(this Event<Func<T, Task>> eventHandler,
        T arg
    ) => eventHandler.Subscriptions.ForEachAsync(x => x(arg));

    public static void Call(this Event<Action> eventHandler)
        => eventHandler.Subscriptions.ForEach(x => x());

    public static void Call<T>(this Event<Action<T>> eventHandler,
        T arg
    ) => eventHandler.Subscriptions.ForEach(x => x(arg));

    public static void Call<T1, T2>(this Event<Action<T1, T2>> eventHandler,
        T1 arg1, T2 arg2
    ) => eventHandler.Subscriptions.ForEach(x => x(arg1, arg2));

    public static void Call<T1, T2, T3>(this Event<Action<T1, T2, T3>> eventHandler,
        T1 arg1, T2 arg2, T3 arg3
    ) => eventHandler.Subscriptions.ForEach(x => x(arg1, arg2, arg3));

    public static void Call<T1, T2, T3, T4>(this Event<Action<T1, T2, T3, T4>> eventHandler,
        T1 arg1, T2 arg2, T3 arg3, T4 arg4
    ) => eventHandler.Subscriptions.ForEach(x => x(arg1, arg2, arg3, arg4));

    public static void Call<T1, T2, T3, T4, T5>(this Event<Action<T1, T2, T3, T4, T5>> eventHandler,
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5
    ) => eventHandler.Subscriptions.ForEach(x => x(arg1, arg2, arg3, arg4, arg5));
}