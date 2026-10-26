using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Rogbau.Api.Models; // Upewnij się, że masz odpowiedni namespace do swoich modeli

namespace Rogbau.Api.Models.Data
{
    public static class DbInitializer
    {
        public const string AdminRole = "Admin";
        public const string MagazynierRole = "Magazynier";
        public const string KierownikBudowyRole = "Kierownik Budowy";

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); // Pobieramy kontekst bazy danych (zmień nazwę, jeśli u Ciebie brzmi inaczej)

            await using var transaction = await context.Database.BeginTransactionAsync();

            string[] roles = [AdminRole, MagazynierRole, KierownikBudowyRole];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException("Nie udało się utworzyć roli: " + role);
                    }
                }
            }

            // Dane pierwszego administratora pobieramy z ustawień Azure.
            // Przy ponownym uruchomieniu nie zmieniamy istniejących kont.
            if (!await userManager.Users.AnyAsync())
            {
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var adminEmail = configuration["BootstrapAdmin:Email"];
                var adminPassword = configuration["BootstrapAdmin:Password"];
                var firstName = configuration["BootstrapAdmin:FirstName"];
                var lastName = configuration["BootstrapAdmin:LastName"];

                if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword) ||
                    string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                {
                    throw new InvalidOperationException("Ustaw BootstrapAdmin:Email, Password, FirstName i LastName przed pierwszym uruchomieniem.");
                }

                var admin = new ApplicationUser();
                admin.UserName = adminEmail.Trim();
                admin.Email = adminEmail.Trim();
                admin.EmailConfirmed = true;
                admin.FirstName = firstName.Trim();
                admin.LastName = lastName.Trim();
                admin.MustChangePassword = true;

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (!result.Succeeded)
                {
                    var messages = new List<string>();
                    foreach (var error in result.Errors)
                    {
                        messages.Add(error.Description);
                    }
                    throw new InvalidOperationException("Nie udało się utworzyć administratora: " + string.Join(" ", messages));
                }

                var roleResult = await userManager.AddToRoleAsync(admin, AdminRole);
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException("Nie udało się nadać roli administratora.");
                }
            }

            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
    {
        new Category
        {
            Name = "Szalunki",
            SubCategories = new List<SubCategory>
            {
                new SubCategory { Name = "Szalunki ścienne" },
                new SubCategory { Name = "Szalunki stropowe" },
                new SubCategory { Name = "Szalunki słupowe" }
            }
        },

        new Category
        {
            Name = "Akcesoria szalunkowe"
        },

        new Category
        {
            Name = "Sprzęt transportowo-magazynowy"
        },

        new Category
        {
            Name = "Elementy ścienne i ryglowane"
        },

        new Category
        {
            Name = "Pozostałe"
        }
    };

                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }


            await transaction.CommitAsync();
        }
    }
}

