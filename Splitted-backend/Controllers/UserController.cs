using AuthenticationServer;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Models.Data_holders;
using Models.DTOs.Incoming;
using Models.DTOs.Outgoing;
using Models.Enums;
using Splitted_backend.Interfaces;
using Splitted_backend.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Splitted_backend.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private ILogger<UserController> logger { get; }

        private IMapper mapper { get; }

        private IRepositoryWrapper repositoryWrapper { get; }

        private AuthenticationManager authenticationManager { get; }


        public UserController(ILogger<UserController> logger, IMapper mapper, IRepositoryWrapper repositoryWrapper, IConfiguration configuration)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.repositoryWrapper = repositoryWrapper;
            this.authenticationManager = new AuthenticationManager(configuration);
        }


        [HttpPost("register")]
        [SwaggerResponse(StatusCodes.Status201Created, "Successfully registered", typeof(UserCreatedDTO))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid body")]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Mail already taken")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegisterDTO userRegisterDTO)
        {
            try
            {
                if (userRegisterDTO is null) 
                    return BadRequest("UserRegisterDTO object is null.");

                if (!ModelState.IsValid) 
                    return BadRequest("Invalid model object.");

                User? userFound = await repositoryWrapper.User.GetEntityOrDefaultByCondition(u => u.Email.Equals(userRegisterDTO.Email));
                if (userFound is not null)
                    return Conflict($"User with mail {userRegisterDTO.Email} already exists.");

                User user = mapper.Map<User>(userRegisterDTO);
                repositoryWrapper.User.Create(user);
                user.UserType = UserTypeEnum.Basic;

                await repositoryWrapper.SaveChanges();

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

                User? user = await repositoryWrapper.User.GetEntityOrDefaultByCondition(u => u.Email.Equals(userLoginDTO.Email));
                if (user is null) 
                    return NotFound($"User with given mail: {userLoginDTO.Email} doesn't exist.");

                if (!user.Password.Equals(userLoginDTO.Password)) 
                    return Unauthorized($"Invalid password for a user with mail: {userLoginDTO.Email}");

                UserLoggedInDTO userLoggedInDTO = new UserLoggedInDTO
                {
                    Token = authenticationManager.GenerateToken(new TokenClaimsData
                    {
                        UserId = user.Id,
                        UserType = user.UserType,
                        Nickname = user.Nickname
                    })
                };
                return Ok(userLoggedInDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside LoginUser method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

    }
}
