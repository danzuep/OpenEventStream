namespace OpenEventStream.Services;

using OpenEventStream.Abstractions;

internal sealed class SystemUtcTicks : ITimestampProvider
{
    public long Ticks => DateTime.UtcNow.Ticks;
    public uint UnixSeconds => (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}
