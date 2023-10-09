using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models.Enums;
using Splitted_backend.Models.Entities;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Splitted_backend.Extensions
{
    public static class UserManagerExtension
    {
        public static async Task AddUserRoles(this UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager, User user,
            List<UserRoleEnum> roleEnums)
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
                new Claim("email", user.Email),
                new Claim("username", user.UserName),
                new Claim("user_id", user.Id.ToString())
            };

            await userManager.AddClaimsAsync(user, userClaims);
        }

        public static async Task<User?> FindByIdWithIncludesAsync(this UserManager<User> userManager, Guid userId, 
            params Expression<Func<User, object>>[] userIncludes)
        {
            return await userManager.Users
                .IncludeMultiple(userIncludes)
                .FirstOrDefaultAsync(u => u.Id.Equals(userId));
        }
    }
}
