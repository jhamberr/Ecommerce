using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Services
{
    public class DatabaseInitializer
    {
        public static async Task SeedDataAsync(UserManager<ApplicationUser>? userManager,
            RoleManager<IdentityRole>? roleManager)
        {
            if (userManager == null || roleManager == null)
            {
                Console.WriteLine("userManager or roleManager is null =. exit");
                return;      
            }

            //Checks to see if we have admin role or not
            /*asynchronous programming. async method so need await at beginning to wait
             * for response or keep programming and return response when it comes*/
            var exists = await roleManager.RoleExistsAsync("admin");
            if (!exists) 
            {
                Console.WriteLine("Admin role is not defined and will be created");
                await roleManager.CreateAsync(new IdentityRole("admin"));//create admin role now
            }

            //check if we have the seller role or note 
            exists = await roleManager.RoleExistsAsync("seller");
            if (!exists)
            {
                Console.WriteLine("Seller role is not defined and will be created");
                await roleManager.CreateAsync(new IdentityRole("seller"));
            }

            //check if we have the client role or note 
            exists = await roleManager.RoleExistsAsync("client");
            if (!exists)
            {
                Console.WriteLine("Client role is not defined and will be created");
                await roleManager.CreateAsync(new IdentityRole("client"));
            }

            //check if we have at least 1 admin user or not
            //a list of users having the role of admin stored in adminUsers if they exist
            var adminUsers = await userManager.GetUsersInRoleAsync("admin");
            if (adminUsers.Any())//if we have any user with role admin
            {
                //Admin user already exists => exit //do not create an admin user
                Console.WriteLine("Admin user already exists => exit");
                return;
            }

            //Create Admin user
            var user = new ApplicationUser()
            {
                FirstName = "Admin",
                LastName = "Admin",
                UserName = "admin@admin.com", //Email will be used to authenticate the user
                Email = "admin@admin.com",
                CreatedAt = DateTime.Now,
            };
            string initialPassword = "admin123";

            /*create user account and provide it with the admin role
             create user with user and initial password */
            
            var result = await userManager.CreateAsync(user, initialPassword);
            if (result.Succeeded)//check if account has been created or not.
            {
                // set the user role
                await userManager.AddToRoleAsync(user, "admin");//make role with user
                Console.WriteLine("Admin user created successfully! Please update the initial password!");
                Console.WriteLine("Email: " + user.Email);
                Console.WriteLine("Initial password: " + initialPassword);    
            }    
        }
    }
}
