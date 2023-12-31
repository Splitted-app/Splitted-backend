﻿using AuthenticationServer.Managers;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Models.DTOs.Incoming.User;
using Models.DTOs.Outgoing.Budget;
using Models.DTOs.Outgoing.User;
using Models.Entities;
using Models.Enums;
using Splitted_backend.Extensions;
using Splitted_backend.Interfaces;
using Splitted_backend.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Splitted_backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private ILogger<UserController> logger { get; }

        private IMapper mapper { get; }

        private IRepositoryWrapper repositoryWrapper { get; }

        private UserManager<User> userManager { get; }

        private RoleManager<IdentityRole<Guid>> roleManager { get; }

        private AuthenticationManager authenticationManager { get; }


        public UserController(ILogger<UserController> logger, IMapper mapper, IRepositoryWrapper repositoryWrapper, UserManager<User> userManager,  
            RoleManager<IdentityRole<Guid>> roleManager, IConfiguration configuration)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.repositoryWrapper = repositoryWrapper;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.authenticationManager = new AuthenticationManager(configuration);
        }


        [HttpPost("register")]
        [SwaggerResponse(StatusCodes.Status201Created, "Successfully registered", typeof(UserCreatedDTO))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid body")]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Mail or username already taken")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegisterDTO userRegisterDTO)
        {
            try
            {
                if (userRegisterDTO is null) 
                    return BadRequest("UserRegisterDTO object is null.");

                if (!ModelState.IsValid) 
                    return BadRequest("Invalid model object.");

                User? userFound = await userManager.FindByEmailAsync(userRegisterDTO.Email);
                if (userFound is not null)
                    return Conflict($"User with mail {userRegisterDTO.Email} already exists.");

                userFound = await userManager.FindByNameAsync(userRegisterDTO.UserName);
                if (userFound is not null)
                    return Conflict($"User with username {userRegisterDTO.UserName} already exists.");

                User user = mapper.Map<User>(userRegisterDTO);
                IdentityResult result = await userManager.CreateAsync(user, userRegisterDTO.Password);

                if (!result.Succeeded)
                    return BadRequest(string.Join("\n", result.Errors.Select(e => e.Description)));

                await userManager.AddUserRoles(roleManager, user, new List<UserRoleEnum> { UserRoleEnum.Member });
                await userManager.AddUserClaims(user);
                
                UserCreatedDTO userCreatedDTO = mapper.Map<UserCreatedDTO>(user);
                return CreatedAtAction("RegisterUser", userCreatedDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside RegisterUser method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("login")]
        [SwaggerResponse(StatusCodes.Status200OK, "Successfully logged in", typeof(UserLoggedInDTO))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid body")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid password")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginDTO userLoginDTO)
        {
            try
            {
                if (userLoginDTO is null) 
                    return BadRequest("UserLoginDTO object is null.");

                if (!ModelState.IsValid) 
                    return BadRequest("Invalid model object.");

                User? user = await userManager.FindByEmailAsync(userLoginDTO.Email);
                if (user is null) 
                    return NotFound($"User with given mail: {userLoginDTO.Email} doesn't exist.");

                if (!await userManager.CheckPasswordAsync(user, userLoginDTO.Password)) 
                    return Unauthorized($"Invalid password for a user with mail: {userLoginDTO.Email}");

                UserLoggedInDTO userLoggedInDTO = new UserLoggedInDTO
                {
                    Token = authenticationManager.GenerateAccessToken(new List<Claim>(await userManager.GetClaimsAsync(user))),
                };

                user.RefreshToken = authenticationManager.GenerateRefreshToken();
                user.RefreshTokenExpiryTime = DateTime.Now.AddHours(24);
                await userManager.UpdateAsync(user);

                Response.Cookies.Append("X-Refresh-Token", user.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                    Secure = true
                });
                return Ok(userLoggedInDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside LoginUser method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("refresh")]
        [SwaggerResponse(StatusCodes.Status200OK, "Successfully refreshed", typeof(UserLoggedInDTO))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Invalid refresh token")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Refresh token not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                if (!Request.Cookies.TryGetValue("X-Refresh-Token", out string? refreshToken))
                    return BadRequest("Refresh token not found.");

                User? userFound = await userManager.FindByRefreshTokenAsync(refreshToken!);
                if (userFound is null || userFound.RefreshTokenExpiryTime <= DateTime.Now)
                    return StatusCode(403, "Invalid refresh token.");

                UserLoggedInDTO userLoggedInDTO = new UserLoggedInDTO
                {
                    Token = authenticationManager.GenerateAccessToken(new List<Claim>(await userManager.GetClaimsAsync(userFound))),
                };

                userFound.RefreshToken = authenticationManager.GenerateRefreshToken();
                userFound.RefreshTokenExpiryTime = DateTime.Now.AddHours(24);
                await userManager.UpdateAsync(userFound);

                Response.Cookies.Append("X-Refresh-Token", userFound.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                    Secure = true,
                });
                return Ok(userLoggedInDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside RefreshToken method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("revoke")]
        [SwaggerResponse(StatusCodes.Status200OK, "Successfully revoked")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> RevokeToken()
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await userManager.UpdateAsync(user);

                Response.Cookies.Delete("X-Refresh-Token");
                return Ok();
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside RevokeToken method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("email-check")]
        [SwaggerResponse(StatusCodes.Status200OK, "Determined if user exists", typeof(UserEmailCheckDTO))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid query parameter")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> EmailCheck([FromQuery, BindRequired] string email)
        {
            try
            {
                if (email is null)
                    return BadRequest("Email is empty.");

                if (!new EmailAddressAttribute().IsValid(email))
                    return BadRequest("Email is invalid.");

                User? userFound = await userManager.FindByEmailAsync(email);

                UserEmailCheckDTO userEmailCheckDTO = new UserEmailCheckDTO
                {
                    UserExists = (userFound is null) ? false : true
                };
                return Ok(userEmailCheckDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside EmailCheck method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        [SwaggerResponse(StatusCodes.Status204NoContent, "User updated")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid body")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Username already taken")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> PutUser([FromBody] UserPutDTO userPutDTO)
        {
            try
            {
                if (userPutDTO is null)
                    return BadRequest("UserPutDTO object is null.");

                if (!ModelState.IsValid)
                    return BadRequest("Invalid model object.");

                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                User? userFound = await userManager.FindByNameAsync(userPutDTO.UserName is null ? 
                    string.Empty : userPutDTO.UserName);
                if (userFound is not null && !user.Equals(userFound))
                    return Conflict($"User with username {userPutDTO.UserName} already exists.");

                mapper.Map(userPutDTO, user);
                await userManager.UpdateAsync(user);

                return NoContent();
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside PutUser method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, "User data returned", typeof(UserGetDTO))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                UserGetDTO userGetDTO = mapper.Map<UserGetDTO>(user);
                return Ok(userGetDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside GetUser method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("budgets")]
        [SwaggerResponse(StatusCodes.Status200OK, "User's budgets returned", typeof(List<BudgetGetDTO>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> GetUserBudgets([FromQuery] BudgetTypeEnum? budgetType)
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, u => u.Budgets);
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                List<Budget> filteredBudgets = user.Budgets
                    .Where(b => budgetType is null || b.BudgetType.Equals(budgetType))
                    .ToList();
                List<BudgetGetDTO> budgets = mapper.Map<List<BudgetGetDTO>>(filteredBudgets);
                return Ok(budgets); 
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside GetUserBudgets method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
