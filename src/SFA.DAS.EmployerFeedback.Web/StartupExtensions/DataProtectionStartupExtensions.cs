using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class DataProtectionStartupExtensions
    {
        public static IServiceCollection AddDasDataProtection(this IServiceCollection services, EmployerFeedbackWebConfiguration configWeb, IHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                return services;
            }

            var redisConnectionString = configWeb.RedisConnectionString;
            var dataProtectionKeysDatabase = configWeb.DataProtectionKeysDatabase;
            var redis = ConnectionMultiplexer.Connect($"{redisConnectionString},{dataProtectionKeysDatabase}");

            services.AddDataProtection()
                .SetApplicationName("das-employer")
                .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");

            return services;
        }
    }
}