using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFeedback.Web.Controllers;

namespace SFA.DAS.EmployerFeedback.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class AddEmployerSharedUiExtensions
    {
        public static IServiceCollection AddEmployerSharedUi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMaMenuConfiguration(ServiceController.SignoutGet, configuration["ResourceEnvironmentName"]);
            return services;
        }
    }
}