using AuthenticationServer;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Incoming;
using Models.DTOs.Outgoing;
using Splitted_backend.Interfaces;
using Splitted_backend.Models.Entities;

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
        public async Task<IActionResult> RegisterUser(UserRegisterDTO userRegisterDTO)
        {
            try
            {
                if (userRegisterDTO is null) return BadRequest("UserRegisterDTO object is null.");

                if (!ModelState.IsValid) return BadRequest("Invalid model object.");

                User user = mapper.Map<User>(userRegisterDTO);

                repositoryWrapper.User.Create(user);
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
        public IActionResult LoginUser()
        {
            string token = authenticationManager.GenerateToken();
            return Ok(new { token = token });
        }

    }
}
