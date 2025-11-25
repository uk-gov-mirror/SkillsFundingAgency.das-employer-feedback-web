using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class CacheStartupExtensions
    {
        public static IServiceCollection AddCache(this IServiceCollection services, IHostEnvironment environment, EmployerFeedbackWebConfiguration config)
        {
            if (environment.IsDevelopment())
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddStackExchangeRedisCache(options => { options.Configuration = config.RedisConnectionString; });
            }
            return services;
        }
    }
}