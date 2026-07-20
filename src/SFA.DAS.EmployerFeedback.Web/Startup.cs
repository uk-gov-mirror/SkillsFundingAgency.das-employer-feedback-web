using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Web.Attributes;
using SFA.DAS.EmployerFeedback.Web.Filters;
using SFA.DAS.EmployerFeedback.Web.ModelBinders;
using SFA.DAS.EmployerFeedback.Web.StartupExtensions;
using SFA.DAS.Validation.Mvc.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Web
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private readonly IHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            _environment = environment;
            _configuration = configuration.BuildDasConfiguration();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(90);
            });
            services.AddConfigurationOptions(_configuration);
            services.AddOpenTelemetryRegistration(_configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]!);
            services.AddAntiforgery(options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            var configurationWeb = _configuration.GetSection<EmployerFeedbackWebConfiguration>();
            var configurationOuterApi = _configuration.GetSection<EmployerFeedbackOuterApiConfiguration>();

            services
                .AddSingleton(configurationWeb)
                .AddSingleton(configurationOuterApi);

            services.AddControllersWithViews();
            services
                .AddMvc(options =>
                {
                    options.AddValidation();
                    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                    options.Filters.Add(new HideAccountNavigationAttribute(false));
                    options.Filters.Add(new EnableGoogleAnalyticsAttribute(_configuration.GetSection<GoogleAnalytics>()));
                    options.Filters.Add(new GoogleAnalyticsFilterAttribute());

                    options.ModelBinderProviders.Insert(0, new AutoDecodeModelBinderProvider());
                })
                .AddControllersAsServices()
                .SetDefaultNavigationSection(NavigationSection.AccountsHome);

            services
                .AddValidatorsFromAssemblyContaining<Startup>();

            services
                .AddEmployerAuthentication(_configuration)
                .AddAuthorizationPolicies()
                .AddSession()
                .AddSessionOptions()
                .AddCache(_environment, configurationWeb)
                .AddMemoryCache()
                .AddCookieTempDataProvider()
                .AddDasDataProtection(configurationWeb, _environment)
                .AddDasHealthChecks(configurationWeb)
                .AddEncodingService()
                .AddServiceRegistrations()
                .AddOuterApi(configurationOuterApi)
                .AddEmployerSharedUi(_configuration)
                .AddSingleton<IActionContextAccessor, ActionContextAccessor>();

#if DEBUG
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
#endif
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env, LinkGenerator linkGenerator)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // re-executes the pipeline for unhandled exceptions at /error
                app.UseExceptionHandler("/error");

                // re-executes the pipeline for non-exception status codes (e.g., 404) at /error/{statusCode}
                app.UseStatusCodePagesWithReExecute("/error/{0}");

                // HSTS configured to 90 days in ConfigureServices.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseDasHealthChecks();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<SecurityHeadersMiddleware>();
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}