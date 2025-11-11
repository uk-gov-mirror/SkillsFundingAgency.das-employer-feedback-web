using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserAccounts;
using SFA.DAS.GovUK.Auth.AppStart;
using SFA.DAS.GovUK.Auth.Configuration;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddEmployerAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IStubAuthenticationService, StubAuthenticationService>(); // TODO can be removed once gov login enabled
            services.Configure<GovUkOidcConfiguration>(configuration.GetSection("GovUkOidcConfiguration"));
            services.AddAndConfigureGovUkAuthentication(configuration,
                new AuthRedirects
                {
                    LocalStubLoginPath = "/SignIn-Stub",
                    SignedOutRedirectUrl = "/home/signedout"
                },
                null,
                typeof(UserAccountsService)
            );
            return services;
        }
    }
}