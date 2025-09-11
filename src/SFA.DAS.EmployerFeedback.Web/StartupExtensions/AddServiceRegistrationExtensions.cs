using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using RestEase.HttpClientFactory;
using SFA.DAS.EmployerFeedback.Infrastructure;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.CacheStorage;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.EmployerAccount;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserAccounts;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Web.Attributes;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Services.EmployerRoleAuthorization;
using SFA.DAS.EmployerProvideFeedback.Infrastructure;
using SFA.DAS.EmployerProvideFeedback.Orchestrators;
using SFA.DAS.EmployerProvideFeedback.Services;
using SFA.DAS.Encoding;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.GovUK.Auth.Employer;
using SFA.DAS.GovUK.Auth.Services;
using SFA.DAS.Http.Configuration;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;

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

            services.AddTransient<IEmployerRoleAuthorizationService, EmployerRoleAuthorizationService>();
            services.AddTransient<IGovAuthEmployerAccountService, UserAccountsService>();

            services.AddTransient<ISessionStorageService, SessionStorageService>();
            services.AddTransient<ICacheStorageService, CacheStorageService>();

            services.AddTransient<IUserService, UserService>();

            services.AddTransient<ValidateRequiredQueryParametersAttribute>();

            services.AddTransient<EnsureFeedbackNotSubmitted>();
            services.AddTransient<EnsureFeedbackNotSubmittedRecentlyAttribute>();
            services.AddTransient<EnsureSessionExists>();
            services.AddTransient<ReviewAnswersOrchestrator>();
            services.AddTransient<ITrainingProviderService, TrainingProviderService>();

            // Encoding Service
            services.AddSingleton<IEncodingService, EncodingService>();
            services.AddTransient<IGovAuthEmployerAccountService, EmployerAccountService>();
            services.AddHttpClient<IOuterApiClient, OuterApiClient>();

            return services;
        }

        public static IServiceCollection AddOuterApi(this IServiceCollection services, EmployerFeedbackOuterApiConfiguration configuration)
        {
            services.AddHealthChecks();
            services.AddScoped<Http.MessageHandlers.DefaultHeadersHandler>();
            services.AddScoped<Http.MessageHandlers.LoggingMessageHandler>();
            services.AddScoped<Http.MessageHandlers.ApimHeadersHandler>();

            services
                .AddRestEaseClient<IEmployerFeedbackOuterApi>(configuration.ApiBaseUrl)
                .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.ApimHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>();

            services.AddTransient<IApimClientConfiguration>((_) => configuration);

            return services;
        }
    }
}