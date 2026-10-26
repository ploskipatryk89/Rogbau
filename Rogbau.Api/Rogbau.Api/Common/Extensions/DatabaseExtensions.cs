using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models;
using Rogbau.Api.Models.Data;
using Rogbau.Api.Common.Identity;

namespace Rogbau.Api.Common.Extensions
{
    public static class DatabaseExtensions
    {

        public static IServiceCollection AddDatabaseAndIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true; //wymaga obecnosci minimum jednej liczby
                options.Password.RequiredLength = 9; //minimalna dlugosc hasla to 10
                options.Password.RequireNonAlphanumeric = true; //czy w hasle wymagany jest znak specjalny
                options.Password.RequireUppercase = true; //czy w hasle wymagana jest minimum jedna wielka litera
            })
                
    .AddErrorDescriber<PolishIdentityErrorDescriber>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


            return services;
        }
    }
}
