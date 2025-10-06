using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class SessionStartupExtensions
    {
        public static IServiceCollection AddSessionOptions(this IServiceCollection services)
        {
            services.AddSession(opt =>
            {
                opt.IdleTimeout = TimeSpan.FromMinutes(60);
                opt.Cookie = new CookieBuilder()
                {
                    Name = "EmployerFeedback.Session",
                    HttpOnly = true
                };
            });

            return services;
        }
    }
}