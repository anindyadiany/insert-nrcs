using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
namespace Insert.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        string[] roles = { "Reporter", "Producer", "AssignmentDesk", "IngestOperator", "Administrator" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
        }

        var testAccounts = new[]
        {
            (Email: "andi@insert.local", Name: "Andi", Role: "Reporter"),
            (Email: "budi@insert.local", Name: "Budi", Role: "Producer"),
            (Email: "desk@insert.local", Name: "Assignment Desk", Role: "AssignmentDesk"),
            (Email: "ingest@insert.local", Name: "Ingest Operator", Role: "IngestOperator"),
            (Email: "admin@insert.local", Name: "Admin", Role: "Administrator"),

            (Email: "rina@insert.local", Name: "Rina", Role: "Reporter"),
            (Email: "dian@insert.local", Name: "Dian", Role: "Reporter"),
            (Email: "sinta@insert.local", Name: "Sinta", Role: "Producer"),
            (Email: "desk2@insert.local", Name: "Assignment Desk 2", Role: "AssignmentDesk"),
        };

        foreach (var (email, name, role) in testAccounts)
        {
            if (await userManager.FindByEmailAsync(email) is not null) continue;

            var user = new ApplicationUser { UserName = email, Email = email, Name = name, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, "Password123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);
        }
    }
}