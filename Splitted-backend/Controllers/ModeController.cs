﻿using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Models.DTOs.Incoming.Budget;
using Models.DTOs.Incoming.Mode;
using Models.DTOs.Outgoing.Budget;
using Models.DTOs.Outgoing.Insights;
using Models.Entities;
using Models.Enums;
using Splitted_backend.EntitiesFilters;
using Splitted_backend.Extensions;
using Splitted_backend.Interfaces;
using Splitted_backend.Managers;
using Splitted_backend.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Security.Claims;

namespace Splitted_backend.Controllers
{
    [ApiController]
    [Route("api/modes")]
    public class ModeController : ControllerBase
    {
        private ILogger<ModeController> logger { get; }

        private IMapper mapper { get; }

        private IRepositoryWrapper repositoryWrapper { get; }

        private UserManager<User> userManager { get; }


        public ModeController(ILogger<ModeController> logger, IMapper mapper,
            IRepositoryWrapper repositoryWrapper, UserManager<User> userManager)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.repositoryWrapper = repositoryWrapper;
            this.userManager = userManager;
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("family-mode/{familyMemberId}")]
        [SwaggerResponse(StatusCodes.Status201Created, "Family mode created", typeof(BudgetCreatedDTO))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid body or invalid family member")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User or family member already in family mode")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User or family member not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> AddFamilyMode([FromRoute, BindRequired] Guid familyMemberId, 
            [FromBody] FamilyModePostDTO familyModePostDTO)
        {
            try
            {
                if (familyModePostDTO is null)
                    return BadRequest("BudgetModePostDTO object is null.");

                if (!ModelState.IsValid)
                    return BadRequest("Invalid model object.");

                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, 
                    (u => u.Budgets, b => ((Budget)b).Transactions, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                if (user.Budgets.Any(b => b.BudgetType.Equals(BudgetTypeEnum.Family)))
                    return StatusCode(403, "User already in family mode.");

                User? familyMember = await userManager.FindByIdWithIncludesAsync(familyMemberId, 
                    (u => u.Budgets, b => ((Budget)b).Transactions, null));
                if (familyMember is null)
                    return NotFound($"Family member with given id: {familyMemberId} doesn't exist.");

                if (familyMember.Id.Equals(user.Id))
                    return BadRequest("You cannot create family mode with yourself.");

                if (familyMember.Budgets.Any(b => b.BudgetType.Equals(BudgetTypeEnum.Family)))
                    return StatusCode(403, "Family member already in family mode.");

                Budget familyBudget = await ModeManager.CreateFamilyMode(repositoryWrapper, user, familyMember,
                    familyModePostDTO);
                BudgetCreatedDTO familyBudgetCreatedDTO = mapper.Map<BudgetCreatedDTO>(familyBudget);

                return CreatedAtAction("AddFamilyMode", familyBudgetCreatedDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside AddFamilyMode method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("partner-mode/{partnerId}")]
        [SwaggerResponse(StatusCodes.Status201Created, "Partner mode created", typeof(BudgetCreatedDTO))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid body or invalid partner")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User or partner already in partner mode")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User or partner not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> AddPartnerMode([FromRoute, BindRequired] Guid partnerId,
            [FromBody] PartnerModePostDTO partnerModePostDTO)
        {
            try
            {
                if (partnerModePostDTO is null)
                    return BadRequest("PartnerModePostDTO object is null.");

                if (!ModelState.IsValid)
                    return BadRequest("Invalid model object.");

                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId,
                    (u => u.Budgets, null, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                if (user.Budgets.Any(b => b.BudgetType.Equals(BudgetTypeEnum.Partner)))
                    return StatusCode(403, "User already in partner mode.");

                User? partner = await userManager.FindByIdWithIncludesAsync(partnerId,
                    (u => u.Budgets, null, null));
                if (partner is null)
                    return NotFound($"Partner with given id: {partnerId} doesn't exist.");

                if (partner.Id.Equals(user.Id))
                    return BadRequest("You cannot create partner mode with yourself.");

                if (partner.Budgets.Any(b => b.BudgetType.Equals(BudgetTypeEnum.Partner)))
                    return StatusCode(403, "Partner already in partner mode.");

                Budget? userFamilyBudget = user.Budgets.FirstOrDefault(b => b.BudgetType.Equals(BudgetTypeEnum.Family));
                if (userFamilyBudget is not null && partner.Budgets.Any(b => b.Id.Equals(userFamilyBudget.Id)))
                    return StatusCode(403, "You cannot create partner mode with your family member.");

                Budget partnerBudget = await ModeManager.CreatePartnerMode(repositoryWrapper, user, partner,
                    partnerModePostDTO);
                BudgetCreatedDTO partnerBudgetCreatedDTO = mapper.Map<BudgetCreatedDTO>(partnerBudget);

                return CreatedAtAction("AddPartnerMode", partnerBudgetCreatedDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside AddPartnerMode method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("temporary-mode/{*userIds}")]
        [SwaggerResponse(StatusCodes.Status201Created, "Temporary mode created", typeof(BudgetCreatedDTO))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid body or invalid user")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> AddTemporaryMode([FromRoute, BindRequired] string userIds,
            [FromBody] TemporaryModePostDTO temporaryModePostDTO)
        {
            try
            {
                if (temporaryModePostDTO is null)
                    return BadRequest("TemporaryModePostDTO object is null.");

                if (!ModelState.IsValid)
                    return BadRequest("Invalid model object.");

                List<Guid> userIdsList = new List<Guid>();
                List<string> userIdsStrings = userIds.Split("/")
                    .ToList();

                foreach (string userIdString in userIdsStrings)
                {
                    Guid otherUserId;
                    bool parsed = Guid.TryParse(userIdString, out otherUserId);
                    if (parsed)
                        userIdsList.Add(otherUserId);
                    else
                        return BadRequest("Some of userIds are invalid.");
                }

                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId,
                    (u => u.Budgets, null, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                List<User> otherUsers = await userManager.FindMultipleByIdsWithIncludesAsync(userIdsList,
                    (u => u.Budgets, null, null));
                if (userIdsList.Count != otherUsers.Count)
                    return NotFound("Some of users were not found.");

                Budget? userFamilyBudget = user.Budgets.FirstOrDefault(b => b.BudgetType.Equals(BudgetTypeEnum.Family));
                if (userFamilyBudget is not null && 
                    otherUsers.Any(ou => ou.Budgets.Any(b => b.Id.Equals(userFamilyBudget.Id))))
                    return StatusCode(403, "You cannot create partner mode with your family member.");

                Budget temporaryBudget = await ModeManager.CreateTemporaryMode(repositoryWrapper, user, otherUsers,
                    temporaryModePostDTO);
                BudgetCreatedDTO temporaryBudgetCreatedDTO = mapper.Map<BudgetCreatedDTO>(temporaryBudget);

                return CreatedAtAction("AddTemporaryMode", temporaryBudgetCreatedDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside AddTemporaryMode method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
