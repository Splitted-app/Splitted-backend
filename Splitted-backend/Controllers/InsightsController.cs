using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Models.DTOs.Outgoing.Budget;
using Models.Entities;
using Splitted_backend.Interfaces;
using Splitted_backend.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Security.Claims;

namespace Splitted_backend.Controllers
{
    [ApiController]
    [Route("api/insights")]
    public class InsightsController : ControllerBase
    {
        private ILogger<BudgetController> logger { get; }

        private IMapper mapper { get; }

        private IRepositoryWrapper repositoryWrapper { get; }

        private UserManager<User> userManager { get; }


        public InsightsController(ILogger<BudgetController> logger, IMapper mapper, 
            IRepositoryWrapper repositoryWrapper, UserManager<User> userManager)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.repositoryWrapper = repositoryWrapper;
            this.userManager = userManager;
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{budgetId}/income-expenses")]
        [SwaggerResponse(StatusCodes.Status200OK, "Income and expenses returned", typeof(BudgetCreatedDTO))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid query parameter")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not a part of the budget")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User or budget not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> GetIncomeExpenses([FromRoute, BindRequired] Guid budgetId, [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo, [FromQuery] string? category)
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByCondition(b => b.Id.Equals(budgetId),
                    (b => b.Transactions, null), (b => b.UserBudgets, null));
                if (budget is null)
                    return NotFound($"Budget with id {budgetId} doesn't exist.");

                bool ifBudgetValid = budget.UserBudgets.Any(ub => ub.UserId.Equals(userId));
                if (!ifBudgetValid)
                    return StatusCode(403, $"User with id {userId} isn't a part of the budget with id {budget.Id}");

                return Ok();

            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside GetIncomeExpenses method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }

        }

    }
}
