using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using ShoeShop.Constants;
using ShoeShop.Models;

namespace ShoeShop.Data;

public static class DbSeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        // 1. Seed Roles
        string[] roles = { Roles.Admin, Roles.User };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Seeded role: {Role}", role);
            }
        }

        // 2. Seed Default Administrator Account
        const string adminEmail = "admin@shoeshop.com";
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Shoe Shop Admin",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            const string adminPassword = "Admin@ShoeShop2026!";
            var createResult = await userManager.CreateAsync(adminUser, adminPassword);

            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                await userManager.AddClaimAsync(adminUser, new Claim(CustomClaims.CanManageInventory, "true"));
                logger.LogInformation("Seeded Default Administrator: {Email}", adminEmail);
            }
            else
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to seed Admin user: {Errors}", errors);
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(existingAdmin, Roles.Admin))
            {
                await userManager.AddToRoleAsync(existingAdmin, Roles.Admin);
            }

            var claims = await userManager.GetClaimsAsync(existingAdmin);
            if (!claims.Any(c => c.Type == CustomClaims.CanManageInventory))
            {
                await userManager.AddClaimAsync(existingAdmin, new Claim(CustomClaims.CanManageInventory, "true"));
            }
        }

        // 3. Seed Sample Shoes with Categories and Colors
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        
        var sampleShoes = new List<Shoe>
        {
            new Shoe
            {
                Brand = "Nike",
                Name = "Air Jordan 1 Retro High OG",
                Category = "Basketball",
                Color = "Red",
                Price = 180.00m,
                ImageUrl = "https://images.unsplash.com/photo-1515955656352-a1fa3ffcd111?auto=format&fit=crop&w=800&q=80"
            },
            new Shoe
            {
                Brand = "Adidas",
                Name = "Ultraboost Light Running",
                Category = "Running",
                Color = "White",
                Price = 190.00m,
                ImageUrl = "https://images.unsplash.com/photo-1587563871167-1ee9c731aefb?auto=format&fit=crop&w=800&q=80"
            },
            new Shoe
            {
                Brand = "New Balance",
                Name = "990v6 Made in USA",
                Category = "Lifestyle",
                Color = "Grey",
                Price = 200.00m,
                ImageUrl = "https://images.unsplash.com/photo-1539185441755-769473a23570?auto=format&fit=crop&w=800&q=80"
            },
            new Shoe
            {
                Brand = "Puma",
                Name = "Suede Classic XXI",
                Category = "Lifestyle",
                Color = "Black",
                Price = 75.00m,
                ImageUrl = "https://images.unsplash.com/photo-1608231387042-66d1773070a5?auto=format&fit=crop&w=800&q=80"
            },
            new Shoe
            {
                Brand = "Nike",
                Name = "Air Zoom Pegasus 40 Flyknit",
                Category = "Running",
                Color = "Blue",
                Price = 140.00m,
                ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=800&q=80"
            },
            new Shoe
            {
                Brand = "Jordan",
                Name = "Luka 2 Space Hunter",
                Category = "Basketball",
                Color = "Black",
                Price = 130.00m,
                ImageUrl = "https://images.unsplash.com/photo-1579338559194-a162d19bf842?auto=format&fit=crop&w=800&q=80"
            },
            new Shoe
            {
                Brand = "Nike",
                Name = "Air Force 1 '07 Triple White",
                Category = "Lifestyle",
                Color = "White",
                Price = 115.00m,
                ImageUrl = "https://images.unsplash.com/photo-1595950653106-6c9ebd614d3a?auto=format&fit=crop&w=800&q=80"
            },
            new Shoe
            {
                Brand = "Adidas",
                Name = "Originals Samba Classic",
                Category = "Lifestyle",
                Color = "Black",
                Price = 100.00m,
                ImageUrl = "https://images.unsplash.com/photo-1518002171953-a080ee817e1f?auto=format&fit=crop&w=800&q=80"
            },
            new Shoe
            {
                Brand = "Puma",
                Name = "Deviate Nitro 2 Carbon",
                Category = "Running",
                Color = "Green",
                Price = 160.00m,
                ImageUrl = "https://images.unsplash.com/photo-1606107557195-0e29a4b5b4aa?auto=format&fit=crop&w=800&q=80"
            },
            new Shoe
            {
                Brand = "Jordan",
                Name = "Air Jordan 4 Retro Bred",
                Category = "Basketball",
                Color = "Red",
                Price = 215.00m,
                ImageUrl = "https://images.unsplash.com/photo-1512374382149-233c42b6a83b?auto=format&fit=crop&w=800&q=80"
            },
            new Shoe
            {
                Brand = "New Balance",
                Name = "550 Vintage White Green",
                Category = "Lifestyle",
                Color = "Green",
                Price = 120.00m,
                ImageUrl = "https://images.unsplash.com/photo-1600185365926-3a2ce3cdb9eb?auto=format&fit=crop&w=800&q=80"
            },
            new Shoe
            {
                Brand = "Adidas",
                Name = "Trae Young 3 Speed",
                Category = "Basketball",
                Color = "Blue",
                Price = 140.00m,
                ImageUrl = "https://images.unsplash.com/photo-1584735935682-2f2b69dff9d2?auto=format&fit=crop&w=800&q=80"
            }
        };

        if (!context.Shoes.Any())
        {
            context.Shoes.AddRange(sampleShoes);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded initial shoe inventory with categories and colors.");
        }
        else
        {
            // Ensure existing shoes have Category and Color populated
            var existing = context.Shoes.ToList();
            foreach (var s in existing)
            {
                if (string.IsNullOrEmpty(s.Category) || s.Category == "Lifestyle")
                {
                    if (s.Name.Contains("Jordan") || s.Name.Contains("Basketball") || s.Brand == "Jordan") s.Category = "Basketball";
                    else if (s.Name.Contains("Running") || s.Name.Contains("Boost") || s.Name.Contains("Pegasus") || s.Name.Contains("Nitro")) s.Category = "Running";
                    else s.Category = "Lifestyle";
                }

                if (string.IsNullOrEmpty(s.Color))
                {
                    if (s.Name.Contains("Red") || s.Name.Contains("Bred")) s.Color = "Red";
                    else if (s.Name.Contains("White") || s.Name.Contains("Light")) s.Color = "White";
                    else if (s.Name.Contains("Blue") || s.Name.Contains("Space")) s.Color = "Blue";
                    else if (s.Name.Contains("Grey") || s.Name.Contains("990")) s.Color = "Grey";
                    else if (s.Name.Contains("Green")) s.Color = "Green";
                    else s.Color = "Black";
                }

                if (s.ImageUrl != null && s.ImageUrl.Contains("photo-1552346154-21d32810aba3"))
                {
                    s.ImageUrl = "https://images.unsplash.com/photo-1515955656352-a1fa3ffcd111?auto=format&fit=crop&w=800&q=80";
                }

                if (s.StockQuantity <= 0)
                {
                    s.StockQuantity = 50;
                }
            }

            // If catalog has fewer than 10 shoes, add additional variety
            if (existing.Count < 10)
            {
                foreach (var s in sampleShoes)
                {
                    if (!existing.Any(e => e.Name == s.Name))
                    {
                        context.Shoes.Add(s);
                    }
                }
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Updated shoe catalog with full category and color classifications.");
        }
    }
}
