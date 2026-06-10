using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YourMatter.Data.Models;

namespace YourMatter.Data.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(YourMatterDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Apply migrations automatically if not already applied
            await context.Database.MigrateAsync();

            // Seed Roles
            string adminRole = "Administrator";
            string userRole = "User";

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            if (!await roleManager.RoleExistsAsync(userRole))
            {
                await roleManager.CreateAsync(new IdentityRole(userRole));
            }

            // Seed Users
            var adminUser = await userManager.FindByEmailAsync("admin@yourmatter.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@yourmatter.com",
                    Email = "admin@yourmatter.com",
                    EmailConfirmed = true,
                    DisplayName = "Admin",
                    Bio = "System administrator of YourMatter.",
                    Location = "Silicon Valley, CA",
                    ProfilePictureUrl = "/images/admin_profile.png",
                    CreatedOn = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "AdminPass123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                }
            }

            var tomUser = await userManager.FindByEmailAsync("tom@myspace.com");
            if (tomUser == null)
            {
                tomUser = new ApplicationUser
                {
                    UserName = "tom@myspace.com",
                    Email = "tom@myspace.com",
                    EmailConfirmed = true,
                    DisplayName = "Tom",
                    Bio = "Hi, I'm Tom. I'm here to help you get started on YourMatter! Feel free to leave a message on my wall.",
                    Location = "Santa Monica, CA",
                    ProfilePictureUrl = "/images/tom.png",
                    CreatedOn = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(tomUser, "TomPass123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(tomUser, userRole);
                }
            }

            var kellyUser = await userManager.FindByEmailAsync("kelly@yourmatter.com");
            if (kellyUser == null)
            {
                kellyUser = new ApplicationUser
                {
                    UserName = "kelly@yourmatter.com",
                    Email = "kelly@yourmatter.com",
                    EmailConfirmed = true,
                    DisplayName = "Kelly",
                    Bio = "Coding and coffee enthusiast. Nostalgic about custom HTML profile themes!",
                    Location = "Seattle, WA",
                    ProfilePictureUrl = "/images/kelly.png",
                    CreatedOn = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(kellyUser, "KellyPass123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(kellyUser, userRole);
                }
            }

            var bobUser = await userManager.FindByEmailAsync("bob@yourmatter.com");
            if (bobUser == null)
            {
                bobUser = new ApplicationUser
                {
                    UserName = "bob@yourmatter.com",
                    Email = "bob@yourmatter.com",
                    EmailConfirmed = true,
                    DisplayName = "Bob",
                    Bio = "Just here to make some new friends and check out retro blogs.",
                    Location = "New York, NY",
                    ProfilePictureUrl = "/images/bob.png",
                    CreatedOn = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(bobUser, "BobPass123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(bobUser, userRole);
                }
            }

            // Reload users with updated states
            adminUser = await userManager.FindByEmailAsync("admin@yourmatter.com");
            tomUser = await userManager.FindByEmailAsync("tom@myspace.com");
            kellyUser = await userManager.FindByEmailAsync("kelly@yourmatter.com");
            bobUser = await userManager.FindByEmailAsync("bob@yourmatter.com");

            if (tomUser != null && kellyUser != null && bobUser != null)
            {
                // Seed Friend Requests
                if (!await context.FriendRequests.AnyAsync())
                {
                    context.FriendRequests.AddRange(new List<FriendRequest>
                    {
                        new FriendRequest
                        {
                            SenderId = tomUser.Id,
                            ReceiverId = kellyUser.Id,
                            Status = FriendRequestStatus.Accepted,
                            SentOn = DateTime.UtcNow.AddDays(-5)
                        },
                        new FriendRequest
                        {
                            SenderId = tomUser.Id,
                            ReceiverId = bobUser.Id,
                            Status = FriendRequestStatus.Accepted,
                            SentOn = DateTime.UtcNow.AddDays(-4)
                        },
                        new FriendRequest
                        {
                            SenderId = kellyUser.Id,
                            ReceiverId = bobUser.Id,
                            Status = FriendRequestStatus.Pending,
                            SentOn = DateTime.UtcNow.AddDays(-1)
                        }
                    });
                    await context.SaveChangesAsync();
                }

                // Ensure all posts, comments, and likes are deleted for a clean presentation
                if (await context.Posts.AnyAsync() || await context.Comments.AnyAsync() || await context.Likes.AnyAsync())
                {
                    context.Comments.RemoveRange(context.Comments);
                    context.Likes.RemoveRange(context.Likes);
                    context.Posts.RemoveRange(context.Posts);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
