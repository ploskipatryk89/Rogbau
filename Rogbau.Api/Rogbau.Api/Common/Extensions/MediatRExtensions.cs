using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Rogbau.Api.Common.Behaviors;
using System.Reflection;

namespace Rogbau.Api.Common.Extensions
{
    public static class MediatRExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // 1. Rejestracja FluentValidation (szuka wszystkich IValidator w projekcie)
            services.AddValidatorsFromAssembly(assembly);

            // 2. Wyłączenie domyślnej walidacji ASP.NET (żeby nasza walidacja w MediatR przejęła kontrolę)
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // 3. MediatR + Behaviors (potok walidacji i logowania)
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);

                // Kolejność ma znaczenie: najpierw Logowanie, potem Walidacja
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            return services;
        }
    }
}
