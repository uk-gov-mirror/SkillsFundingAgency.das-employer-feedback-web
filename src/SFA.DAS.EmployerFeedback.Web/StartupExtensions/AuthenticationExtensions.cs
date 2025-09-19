using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.GovUK.Auth.AppStart;
using SFA.DAS.GovUK.Auth.Configuration;
using SFA.DAS.GovUK.Auth.Services;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.GovUK.Auth.Models;

namespace SFA.DAS.EmployerFeedback.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddEmployerAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IStubAuthenticationService, StubAuthenticationService>(); // TODO can be removed once gov login enabled
            services.Configure<GovUkOidcConfiguration>(configuration.GetSection("GovUkOidcConfiguration"));
            //services.AddAndConfigureGovUkAuthentication(configuration, 
            //    new AuthRedirects
            //    {
            //      LocalStubLoginPath  = "/SignIn-Stub",
            //      SignedOutRedirectUrl = ""
            //    },
            //    null,
            //    typeof(UserAccountsService)
            //    );

            return services;
        }
    }
}