namespace RyuBot.Helpers;

// https://stackoverflow.com/a/31194647
public sealed class AsyncDuplicateLock<T>
{
    private sealed class RefCounted<TR>
    {
        public RefCounted([NotNull] TR value)
        {
            RefCount = 1;
            Value = value;
        }

        public int RefCount { get; set; }
        public TR Value { get; }
    }

    private readonly Dictionary<T, RefCounted<SemaphoreSlim>> _semaphores
        = new();

    [return: NotNull]
    private SemaphoreSlim GetOrCreate(T key)
    {
        RefCounted<SemaphoreSlim> item;
        lock (_semaphores)
        {
            if (_semaphores.TryGetValue(key, out item))
            {
                ++item.RefCount;
            }
            else
            {
                item = new(new(1, 1));
                _semaphores[key] = item;
            }
        }

        return item.Value;
    }

    public IDisposable Lock(T key)
    {
        GetOrCreate(key).Wait();
        return new Releaser(key, _semaphores);
    }

    public async Task<IDisposable> LockAsync(T key)
    {
        await GetOrCreate(key).WaitAsync();
        return new Releaser(key, _semaphores);
    }

    private readonly struct Releaser(T key, Dictionary<T, RefCounted<SemaphoreSlim>> semaphores) : IDisposable
    {
        public T Key { get; } = key;

        public void Dispose()
        {
            RefCounted<SemaphoreSlim> item;
            lock (semaphores)
            {
                item = semaphores[Key];
                --item.RefCount;
                if (item.RefCount is 0) semaphores.Remove(Key);
            }

            item.Value.Release();
        }
    }
}