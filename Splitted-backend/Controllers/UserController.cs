using AuthenticationServer;
using Microsoft.AspNetCore.Mvc;
using Splitted_backend.Interfaces;

namespace Splitted_backend.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private ILogger<UserController> logger { get; }

        private IRepositoryWrapper repositoryWrapper { get; }

        private AuthenticationManager authenticationManager { get; }


        public UserController(ILogger<UserController> logger, IRepositoryWrapper repositoryWrapper, IConfiguration configuration)
        {
            this.logger = logger;
            this.repositoryWrapper = repositoryWrapper;
            this.authenticationManager = new AuthenticationManager(configuration);
        }


        [HttpPost("login")]
        public IActionResult LoginUser()
        {
            string token = authenticationManager.GenerateToken();
            return Ok(new { token = token });
        }

    }
}
