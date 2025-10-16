using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Encoding;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class AddEncodingServiceExtensions
    {
        public static IServiceCollection AddEncodingService(this IServiceCollection services)
        {
            services.AddSingleton<IEncodingService, EncodingService>();
            return services;
        }
    }
}