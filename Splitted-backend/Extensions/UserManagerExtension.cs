using Microsoft.AspNetCore.Identity;
using Models.Enums;
using Splitted_backend.Models.Entities;
using System.Security.Claims;

namespace Splitted_backend.Extensions
{
    public static class UserManagerExtension
    {
        public static async Task AddUserRoles(this UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager, List<UserRoleEnum> roleEnums, 
            User user)
        {
            foreach (UserRoleEnum roleEnum in roleEnums)
            {
                string role = roleEnum.ToString();

                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));

                await userManager.AddToRoleAsync(user, role);
            }
        }

        public static async Task AddUserClaims(this UserManager<User> userManager, User user)
        {
            List<Claim> userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            await userManager.AddClaimsAsync(user, userClaims);
        }
    }
}
