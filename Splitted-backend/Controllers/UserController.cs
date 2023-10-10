using AuthenticationServer.Managers;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Models.DTOs.Incoming.User;
using Models.DTOs.Outgoing.User;
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

                userFound = await userManager.FindByNameAsync(userRegisterDTO.Username);
                if (userFound is not null)
                    return Conflict($"User with username {userRegisterDTO.Username} already exists.");

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
                    Token = authenticationManager.GenerateToken(new List<Claim>(await userManager.GetClaimsAsync(user)))
                };
                return Ok(userLoggedInDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside LoginUser method. {exception}.");
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
    }
}
