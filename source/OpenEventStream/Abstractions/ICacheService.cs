namespace OpenEventStream.Abstractions
{
    public interface ICacheService<T> : IEnumerable<KeyValuePair<string, T>>, IDisposable
    {
        T? GetOrAdd(string key, Func<string, T> valueFactory);
    }
}