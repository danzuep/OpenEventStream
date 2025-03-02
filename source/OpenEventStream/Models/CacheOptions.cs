namespace OpenEventStream.Models;

public sealed class CacheOptions
{
    public TimeSpan Expiry { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);
    public bool UseCompositeKey { get; set; } = true;
    public string Delimiter { get; set; } = ":";
}
