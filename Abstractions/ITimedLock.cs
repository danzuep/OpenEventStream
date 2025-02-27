namespace OpenEventStream.Abstractions
{
    public interface ITimedLock
    {
        /// <inheritdoc cref="Monitor.TryEnter(object, TimeSpan)"/>
        bool TryExecute(Action action, TimeSpan? timeout = null, object? syncLock = null);
    }
}