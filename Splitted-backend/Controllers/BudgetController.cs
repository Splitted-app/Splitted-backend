using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Incoming.Budget;
using Models.DTOs.Outgoing.Budget;
using Models.Entities;
using Splitted_backend.Interfaces;
using Splitted_backend.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Splitted_backend.Controllers
{
    [ApiController]
    [Route("api/budget")]
    public class BudgetController : ControllerBase
    {
        private ILogger<BudgetController> logger { get; }

        private IMapper mapper { get; }

        private IRepositoryWrapper repositoryWrapper { get; }

        private UserManager<User> userManager { get; }


        public BudgetController(ILogger<BudgetController> logger, IMapper mapper, IRepositoryWrapper repositoryWrapper, UserManager<User> userManager)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.repositoryWrapper = repositoryWrapper;
            this.userManager = userManager;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [SwaggerResponse(StatusCodes.Status201Created, "Budget created")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid body")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> CreateBudget([FromBody] BudgetPostDTO budgetPostDTO)
        {
            try
            {
                if (budgetPostDTO is null)
                    return BadRequest("BudgetPostDTO object is null.");

                if (!ModelState.IsValid)
                    return BadRequest("Invalid model object.");

                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                Budget budget = mapper.Map<Budget>(budgetPostDTO);

                repositoryWrapper.Budgets.Create(budget);
                user.Budgets.Add(budget);
                await repositoryWrapper.SaveChanges();

                BudgetCreatedDTO budgetCreatedDTO = mapper.Map<BudgetCreatedDTO>(budget);
                return CreatedAtAction("CreateBudget", budgetCreatedDTO);

            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside CreateBudget method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

    }
}
