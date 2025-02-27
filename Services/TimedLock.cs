namespace OpenEventStream.Services;
using OpenEventStream.Abstractions;

public sealed class TimedLock : ITimedLock
{
    public bool TryExecute(Action action, TimeSpan? timeout = null, object? syncLock = null)
    {
        timeout ??= TimeSpan.FromSeconds(1);
        syncLock ??= new object();
        if (!Monitor.TryEnter(syncLock, timeout.Value))
        {
            return false;
        }
        try
        {
            action();
        }
        finally
        {
            Monitor.Exit(syncLock);
        }
        return true;
    }
}
