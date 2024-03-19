using BlogDotNet8.Data;
using BlogDotNet8.Enums;
using BlogDotNet8.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogDotNet8.Services
{
    public class DataService
    {
        //
        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BlogUser> _userManager;

        public DataService(ApplicationDbContext dbContext,
                           RoleManager<IdentityRole> roleManager, 
                           UserManager<BlogUser> userManager)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
        }



        public async Task ManageDataAsync()
        {
            //Task: Create the DB from the Migrations
            //equivalent to locally running update database 
           await  _dbContext.Database.MigrateAsync();
            //Task 1: Seeding a few roles into the system
            await SeedRolesAsync();


            //Task 2: Seed a user into the system (use class to programatically add a user for me)
            await SeedUsersAsync();
        }

        private async Task SeedRolesAsync()
        {
            //if there are already roles in the system do nothing

            if (_dbContext.Roles.Any())

            {
                return;
            }
            //otherewise we want to create a few roles
            foreach(var role in Enum.GetNames(typeof(BlogRole)))
            {
                //I need to use the role manager to create roles
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

        }

        private async Task SeedUsersAsync()
        {
            //if there are already users in the system do nothing
            if (_dbContext.Users.Any())
            {
                return;
            }
            //step one creates a new instance of Bloguser
            var adminUser = new BlogUser()
            {
                Email = "codedbywells@gmail.com",
                UserName = "codedbywells@gmail.com",
                FirstName = "Joshua",
                LastName = "Wells",
                DisplayName = "Josh The Admin",
                PhoneNumber = "(281) 217-1876",
                EmailConfirmed = true,
                
            };

            //step 2 use UserManager to create a new user that is defined by the adminUser variable
            await _userManager.CreateAsync(adminUser, "Abc&123!");

            //Step 3 add this new user the the administrator role
            await _userManager.AddToRoleAsync(adminUser, BlogRole.Administrator.ToString());

            //Step 1 Repeat: Create the moderator user
            var modUser = new BlogUser()
            {
                Email = "wellsejoshua@outlook.com",
                UserName = "wellsejoshua@outlook.com",
                FirstName = "Josh Mod",
                LastName = "Wells Mod",
                DisplayName = "Josh The Moderator",
                PhoneNumber = "(281) 217-1876",
                EmailConfirmed = true

            };

            await _userManager.CreateAsync(modUser, "Abc&123!");
            await _userManager.AddToRoleAsync(modUser, BlogRole.Moderator.ToString());


        }


        //Task 2: Seed a user into the system (use class to programatically add a user for me)
        //Task 3:

    }
}
