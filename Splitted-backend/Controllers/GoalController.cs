using AIService;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Models.DTOs.Incoming.Budget;
using Models.DTOs.Incoming.Goal;
using Models.DTOs.Outgoing.Budget;
using Models.DTOs.Outgoing.Goal;
using Models.Entities;
using Models.Enums;
using Splitted_backend.Extensions;
using Splitted_backend.Interfaces;
using Splitted_backend.Managers;
using Splitted_backend.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Splitted_backend.Controllers
{
    [ApiController]
    [Route("api/goals")]
    public class GoalController : ControllerBase
    {
        private ILogger<GoalController> logger { get; }

        private IMapper mapper { get; }

        private IRepositoryWrapper repositoryWrapper { get; }

        private UserManager<User> userManager { get; }


        public GoalController(ILogger<GoalController> logger, IMapper mapper, IRepositoryWrapper repositoryWrapper,
            UserManager<User> userManager)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.repositoryWrapper = repositoryWrapper;
            this.userManager = userManager;
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [SwaggerResponse(StatusCodes.Status201Created, "Goal created", typeof(GoalCreatedDTO))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid body")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> CreateGoal([FromBody] GoalPostDTO goalPostDTO)
        {
            try
            {
                if (goalPostDTO is null)
                    return BadRequest("GoalPostDTO object is null.");

                if (!ModelState.IsValid)
                    return BadRequest("Invalid model object.");

                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.Goals, null, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                if (((DateTime)goalPostDTO.Deadline - DateTime.Today).Days <= 0)
                    return BadRequest("Deadline has to be greater than today's date.");

                Goal goal = mapper.Map<Goal>(goalPostDTO);
                GoalManager.SetGoalName(goalPostDTO, goal);

                repositoryWrapper.Goals.Create(goal);
                user.Goals.Add(goal);
                await repositoryWrapper.SaveChanges();

                GoalCreatedDTO goalCreatedDTO = mapper.Map<GoalCreatedDTO>(goal);
                return CreatedAtAction("CreateGoal", goalCreatedDTO);

            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside CreateGoal method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("{goalId}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Goal updated")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid body")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Goal doesn't belong to user")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User or goal not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> PutGoal([FromBody] GoalPutDTO goalPutDTO,
            [FromRoute, BindRequired] Guid goalId)
        {
            try
            {
                if (goalPutDTO is null)
                    return BadRequest("GoalPutDTO object is null.");

                if (!ModelState.IsValid)
                    return BadRequest("Invalid model object.");

                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.Goals, null, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                Goal? goal = await repositoryWrapper.Goals.GetEntityOrDefaultByConditionAsync(g => g.Id.Equals(goalId));
                if (goal is null)
                    return NotFound($"Goal with given id: {goalId} doesn't exist.");

                if (!user.Goals.Any(g => g.Id.Equals(goal.Id)))
                    return StatusCode(403, $"Goal with id: {goal.Id} doesn't belong to user.");

                if (goalPutDTO.Deadline is not null && ((DateTime)goalPutDTO.Deadline - DateTime.Today).Days <= 0)
                    return BadRequest("Deadline has to be greater than today's date.");

                if (goalPutDTO.IsMain ?? false)
                    user.Goals.ForEach(g => g.IsMain = false);

                mapper.Map(goalPutDTO, goal);
                repositoryWrapper.Goals.Update(goal);
                await repositoryWrapper.SaveChanges();

                return NoContent();
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside PutGoal method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("{*goalIds}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Goals deleted")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid path parameter")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Some of goals don't belong to user")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User or goals not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> DeleteGoals([FromRoute, BindRequired] string goalIds)
        {
            try
            {
                List<Guid> goalIdsList = new List<Guid>();
                List<string> goalIdsStrings = goalIds.Split("/")
                    .ToList();

                foreach (string goalIdString in goalIdsStrings)
                {
                    Guid goalId;
                    bool parsed = Guid.TryParse(goalIdString, out goalId);
                    if (parsed)
                        goalIdsList.Add(goalId);
                    else
                        return BadRequest("Some of goalIds are invalid.");
                }

                List<Goal> goals = await repositoryWrapper.Goals.GetEntitiesByConditionAsync(g => goalIdsList.Contains(g.Id));
                if (goalIdsList.Count != goals.Count)
                    return NotFound("Some of goals were not found.");

                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.Goals, null, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                if (!goals.All(g => user.Goals.Any(ug => ug.Id.Equals(g.Id))))
                    return StatusCode(403, "Some of goals don't belong to user.");

                repositoryWrapper.Goals.DeleteMultiple(goals);
                await userManager.UpdateAsync(user);

                return NoContent();
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside DeleteGoals method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
