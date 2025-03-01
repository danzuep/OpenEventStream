namespace OpenEventStream.Abstractions
{
    public interface ITimestampProvider
    {
        long Ticks { get; }
    }
}
