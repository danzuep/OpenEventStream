using Microsoft.Extensions.DependencyInjection;
using OpenEventStream.Abstractions;
using OpenEventStream.Services;
using System.Diagnostics.CodeAnalysis;

namespace Data.Service.Extensions
{
    [ExcludeFromCodeCoverage]
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventStreamServices(this IServiceCollection services)
        {
            services.AddScoped<ITimestampProvider, SystemUtcTicks>();
            services.AddScoped<ITimedLock, TimedLock>();
            services.AddScoped<ICacheServiceFactory, CacheServiceFactory>();
            services.AddScoped<ITimeSeriesFactory, TimeSeriesFactory>();
            services.AddScoped<ITimeSeriesBrokerFactory, TimeSeriesBrokerFactory>();
            //using var serviceProvider = services.BuildServiceProvider();
            //using var scope = serviceProvider.CreateScope();
            return services;
        }
    }
}
