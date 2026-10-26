using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Rogbau.Api.Models;
using System.Security.Claims;
using Rogbau.Api.Configuration;
using Rogbau.Api.Features.EquipmentTransfer.Services;
using Rogbau.Api.Services;
using System.Text;

namespace Rogbau.Api.Common.Extensions
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Konfiguracja JwtOptions dla DI
            services.Configure<JwtOptions>(
                configuration.GetSection(JwtOptions.SectionName));

            // Serwisy związane z autoryzacją
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<NumberGenerator>();

            // Pobranie konfiguracji JWT
            var jwtSection =
                configuration.GetSection(JwtOptions.SectionName);

            var secret = jwtSection["Secret"];
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];

            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException(
                    "Nie udało się odczytać 'Secret' z sekcji Jwt.");
            }

            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new InvalidOperationException(
                    "Nie udało się odczytać 'Issuer' z sekcji Jwt.");
            }

            if (string.IsNullOrWhiteSpace(audience))
            {
                throw new InvalidOperationException(
                    "Nie udało się odczytać 'Audience' z sekcji Jwt.");
            }

            if (Encoding.UTF8.GetByteCount(secret) < 32)
            {
                throw new InvalidOperationException("Klucz Jwt:Secret musi mieć co najmniej 32 bajty.");
            }
            int expiry = configuration.GetValue<int>("Jwt:ExpiryInMinutes");
            if (expiry <= 0)
            {
                throw new InvalidOperationException("Jwt:ExpiryInMinutes musi być większe od zera.");
            }

            var key = Encoding.UTF8.GetBytes(secret);

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        JwtBearerDefaults.AuthenticationScheme;

                    options.DefaultChallengeScheme =
                        JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidIssuer = issuer,
                            ValidAudience = audience,

                            IssuerSigningKey =
                                new SymmetricSecurityKey(key),

                            ClockSkew = TimeSpan.Zero
                        };

                    // WAŻNE DLA SIGNALR
                    //
                    // Przy połączeniu WebSocket SignalR może przekazać
                    // JWT jako:
                    //
                    // /hubs/notifications?access_token=...
                    //
                    // Dlatego musimy ręcznie przekazać ten token
                    // do JwtBearer.
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                            var stamp = context.Principal?.FindFirstValue("securityStamp");
                            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(stamp))
                            {
                                context.Fail("Sesja wygasła. Zaloguj się ponownie.");
                                return;
                            }

                            var user = await userManager.FindByIdAsync(userId);
                            if (user == null || stamp != await userManager.GetSecurityStampAsync(user))
                            {
                                context.Fail("Sesja wygasła. Zaloguj się ponownie.");
                                return;
                            }

                            var currentRoles = await userManager.GetRolesAsync(user);
                            var tokenRoles = context.Principal!.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                            if (tokenRoles.Count != currentRoles.Count || tokenRoles.Any(role => !currentRoles.Contains(role)))
                                context.Fail("Uprawnienia zostały zmienione. Zaloguj się ponownie.");
                        },
                        OnMessageReceived = context =>
                        {
                            var accessToken =
                                context.Request.Query["access_token"];

                            var path =
                                context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments(
                                    "/hubs/notifications"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }
    }
}

