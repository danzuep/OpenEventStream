using OpenEventStream.Models;

namespace OpenEventStream.Abstractions
{
    public interface ICacheServiceFactory
    {
        ICacheService<T> Create<T>(CacheOptions? cacheOptions = null);
    }
}