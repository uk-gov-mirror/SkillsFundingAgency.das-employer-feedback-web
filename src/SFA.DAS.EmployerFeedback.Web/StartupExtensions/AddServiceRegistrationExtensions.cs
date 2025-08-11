using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using RestEase.HttpClientFactory;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.CacheStorage;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Web.Attributes;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Services.EmployerRoleAuthorization;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.Http.Configuration;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.GovUK.Auth.Employer;
using SFA.DAS.EmployerFeedback.Infrastructure.Api;

namespace SFA.DAS.EmployerFeedback.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class AddServiceRegistrationExtensions
    {
        public static IServiceCollection AddServiceRegistrations(this IServiceCollection services)
        {
            //services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetExistingEmployerRequestQuery).Assembly));

            services.AddSingleton<IAuthorizationHandler, OwnerRoleAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, TransactorRoleAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewerRoleAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, NoneRoleAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, AccountActiveAuthorizationHandler>();

            //services.AddTransient<IEmployerRoleAuthorizationService, EmployerRoleAuthorizationService>();
            //services.AddTransient<IGovAuthEmployerAccountService, UserAccountsService>();

            services.AddTransient<ISessionStorageService, SessionStorageService>();
            services.AddTransient<ICacheStorageService, CacheStorageService>();

            services.AddTransient<IUserService, UserService>();

            services.AddTransient<ValidateRequiredQueryParametersAttribute>();

            return services;
        }

        public static IServiceCollection AddOuterApi(this IServiceCollection services, EmployerFeedbackOuterApiConfiguration configuration)
        {
            services.AddHealthChecks();
            services.AddScoped<Http.MessageHandlers.DefaultHeadersHandler>();
            services.AddScoped<Http.MessageHandlers.LoggingMessageHandler>();
            services.AddScoped<Http.MessageHandlers.ApimHeadersHandler>();

            services
                .AddRestEaseClient<ICommitmentsOuterApi>(configuration.ApiBaseUrl)
                .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.ApimHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>();

            services.AddTransient<IApimClientConfiguration>((_) => configuration);

            return services;
        }
    }
}