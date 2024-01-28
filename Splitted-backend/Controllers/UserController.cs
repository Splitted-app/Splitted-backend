using AIService;
using AuthenticationServer.Managers;
using AutoMapper;
using ExternalServices.EmailSender;
using ExternalServices.StorageClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MimeKit.Encodings;
using Models.DTOs.Incoming.User;
using Models.DTOs.Outgoing.Budget;
using Models.DTOs.Outgoing.Goal;
using Models.DTOs.Outgoing.User;
using Models.EmailModels;
using Models.Entities;
using Models.Enums;
using Splitted_backend.Extensions;
using Splitted_backend.Interfaces;
using Splitted_backend.Managers;
using Splitted_backend.Models.Entities;
using Splitted_backend.Utils.TimeProvider;
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

        private ITimeProvider timeProvider { get; }

        private IRepositoryWrapper repositoryWrapper { get; }

        private IEmailSender emailSender { get; }

        private IStorageClient storageClient { get; }

        private UserManager<User> userManager { get; }

        private RoleManager<IdentityRole<Guid>> roleManager { get; }

        private BaseAuthenticationManager authenticationManager { get; }


        public UserController(ILogger<UserController> logger, IMapper mapper, ITimeProvider timeProvider, 
            IRepositoryWrapper repositoryWrapper, IEmailSender emailSender, IStorageClient storageClient,
            UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager,
            BaseAuthenticationManager authenticationManager)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.timeProvider = timeProvider;
            this.repositoryWrapper = repositoryWrapper;
            this.emailSender = emailSender;
            this.storageClient = storageClient;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.authenticationManager = authenticationManager;
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

                string token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                await emailSender.SendConfirmationEmail(token, user.Email);

                UserCreatedDTO userCreatedDTO = mapper.Map<UserCreatedDTO>(user);
                return CreatedAtAction("RegisterUser", userCreatedDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside RegisterUser method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("confirm-email")]
        [SwaggerResponse(StatusCodes.Status200OK, "Confirmed email")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid token")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> ConfirmEmail([FromQuery, BindRequired] string token, 
            [FromQuery, BindRequired] string email)
        {
            User user = await userManager.FindByEmailAsync(email);
            if (user is null)
                return NotFound($"User with email {email} doesn't exist.");

            IdentityResult result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return Unauthorized(string.Join("\n", result.Errors.Select(e => e.Description)));

            return Ok();
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
                    Token = authenticationManager.GenerateAccessToken(
                        new List<Claim>(await userManager.GetClaimsAsync(user))),
                };

                user.RefreshToken = authenticationManager.GenerateRefreshToken();
                user.RefreshTokenExpiryTime = DateTime.Now.AddHours(24);
                await userManager.UpdateAsync(user);

                Response.Cookies.Append("X-Refresh-Token", user.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
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
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid refresh token")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Refresh token not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                if (!Request.Cookies.TryGetValue("X-Refresh-Token", out string? refreshToken))
                    return NotFound("Refresh token not found.");

                User? userFound = await userManager.FindByRefreshTokenAsync(refreshToken!);
                if (userFound is null || userFound.RefreshTokenExpiryTime <= DateTime.Now)
                    return Unauthorized("Invalid refresh token.");

                UserLoggedInDTO userLoggedInDTO = new UserLoggedInDTO
                {
                    Token = authenticationManager.GenerateAccessToken(
                        new List<Claim>(await userManager.GetClaimsAsync(userFound))),
                };

                userFound.RefreshToken = authenticationManager.GenerateRefreshToken();
                userFound.RefreshTokenExpiryTime = DateTime.Now.AddHours(24);
                await userManager.UpdateAsync(userFound);

                Response.Cookies.Append("X-Refresh-Token", userFound.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
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

                if (userPutDTO.AvatarImage is not null)
                {
                    userPutDTO.AvatarImage = await storageClient.UploadProfilePicture(userPutDTO.AvatarImage);
                    if (userPutDTO.AvatarImage is null)
                        return BadRequest("Invalid profile picture.");
                }

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
        [HttpDelete]
        [SwaggerResponse(StatusCodes.Status204NoContent, "User deleted")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> DeleteUser()
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId,
                    (u => u.Budgets, b => ((Budget)b).Transactions, t => ((Transaction)t).TransactionPayBacks), 
                    (u => u.Budgets, b => ((Budget)b).Users, null),
                    (u => u.Friends, null, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                user.Budgets
                    .Where(b => b.BudgetType.Equals(BudgetTypeEnum.Partner) ||
                        b.BudgetType.Equals(BudgetTypeEnum.Temporary))
                    .ToList()
                    .ForEach(b => b.Transactions.ForEach(t => t.TransactionPayBacks.RemoveAll(tpb =>
                        tpb.OwingUserId.Equals(userId) || tpb.OwedUserId.Equals(userId))));
                    
                user.Budgets.ForEach(b => ModeManager.LeaveMode(repositoryWrapper, null, 
                    b.Users.Where(u => !u.Id.Equals(userId)).ToList(),
                    b));

                repositoryWrapper.Budgets.DeleteMultiple(user.Budgets.Where(b => b.Users.Count() <= 2));

                await userManager.DeleteAsync(user);
                return NoContent();
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside DeleteUser method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("budgets")]
        [SwaggerResponse(StatusCodes.Status200OK, "User's budgets returned", typeof(List<UserBudgetGetDTO>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid query parameter")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> GetUserBudgets([FromQuery] string? budgetType)
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.Budgets, null, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                List<BudgetTypeEnum> budgetTypeEnums = new List<BudgetTypeEnum>();
                if (budgetType is not null)
                {
                    string[] budgetTypesStrings = budgetType.Split(",");
                    foreach (string budgetTypeString in budgetTypesStrings)
                    {
                        BudgetTypeEnum budgetTypeEnum;
                        bool parsed = Enum.TryParse(budgetTypeString, true, out budgetTypeEnum);

                        if (parsed)
                            budgetTypeEnums.Add(budgetTypeEnum);
                        else
                            return BadRequest("Invalid query paremeter.");
                    }
                }

                List<Budget> filteredBudgets = user.Budgets
                    .Where(b => budgetType is null || budgetTypeEnums.Any(bte => bte.Equals(b.BudgetType)))
                    .ToList();
                List<UserBudgetGetDTO> budgets = mapper.Map<List<UserBudgetGetDTO>>(filteredBudgets);

                return Ok(budgets); 
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside GetUserBudgets method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("budgets/{budgetId}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Budget left")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid path parameter")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not a part of the budget or invalid budget type")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User or budget not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> LeaveBudget([FromRoute, BindRequired] Guid budgetId)
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.Budgets, null, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                Budget? budget = await repositoryWrapper.Budgets
                    .GetEntityOrDefaultByConditionAsync(b => b.Id.Equals(budgetId), 
                    (b => b.Users, null, null), 
                    (b => b.Transactions, 
                    t => ((Transaction)t).TransactionPayBacks, 
                    null));
                if (budget is null)
                    return NotFound($"Budget with given id: {budgetId} doesn't exist.");

                bool ifBudgetValid = budget.Users.Any(u => u.Id.Equals(userId));
                if (!ifBudgetValid)
                    return StatusCode(403, $"User with id {userId} isn't a part of the budget with id {budget.Id}");

                if (budget.BudgetType.Equals(BudgetTypeEnum.Personal))
                    return StatusCode(403, "You cannot leave a personal budget.");

                List<User> otherUsers = budget.Users
                    .Where(u => !u.Id.Equals(userId))
                    .ToList();
                ModeManager.LeaveMode(repositoryWrapper, user, otherUsers, budget);

                if (budget.Users.Count() == 2)
                    repositoryWrapper.Budgets.Delete(budget);

                await repositoryWrapper.SaveChanges();
                return NoContent();
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside LeaveBudget method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("search")]
        [SwaggerResponse(StatusCodes.Status200OK, "Users returned", typeof(List<UserGetDTO>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> Search([FromQuery, BindRequired] string query)
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.Friends, null, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                List<User> usersFound = SearchManager.SearchUsers(userManager.Users, query, userId);
                List<UserGetDTO> usersFoundDTOs = mapper.Map<List<UserGetDTO>>(usersFound);

                return Ok(usersFoundDTOs);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside Search method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("friends")]
        [SwaggerResponse(StatusCodes.Status200OK, "Friends returned", typeof(List<UserGetDTO>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> GetUserFriends()
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.Friends, null, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                List<UserGetDTO> friends = mapper.Map<List<UserGetDTO>>(user.Friends);
                return Ok(friends);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside GetUserFriends method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("friends/{friendId}")]
        [SwaggerResponse(StatusCodes.Status201Created, "Friend added")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid friend")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User or friend not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> AddFriend([FromRoute, Required] Guid friendId)
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.Friends, null, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                User? friend = await userManager.FindByIdWithIncludesAsync(friendId);
                if (friend is null)
                    return NotFound($"Friend with given id: {friendId} doesn't exist.");

                if (user.Id == friend.Id)
                    return BadRequest("You cannot add yourself to your friends.");

                if (user.Friends.Contains(friend))
                    return BadRequest($"Friend with given id: {friendId} already added.");

                user.Friends.Add(friend);
                friend.Friends.Add(user);

                await repositoryWrapper.SaveChanges();
                return CreatedAtAction("AddFriend", null);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside AddFriend method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("friends/{*friendIds}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Friends deleted")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid friend")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User or friend not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> DeleteFriends([FromRoute, Required] string friendIds)
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.Friends, null, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                List<Guid> friendIdsList = new List<Guid>();
                List<string> friendsIdsStrings = friendIds.Split("/")
                    .ToList();

                foreach (string friendIdString in friendsIdsStrings)
                {
                    Guid friendId;
                    bool parsed = Guid.TryParse(friendIdString, out friendId);
                    if (parsed)
                        friendIdsList.Add(friendId);
                    else
                        return BadRequest("Some of friendIds are invalid.");
                }

                List<User> friends = await userManager.FindMultipleByIdsWithIncludesAsync(friendIdsList, 
                    (u => u.Friends, null, null));
                if (friendIdsList.Count != friends.Count)
                    return NotFound("Some of friends were not found.");

                foreach (User friend in friends)
                {
                    if (!user.Friends.Contains(friend))
                        return BadRequest($"User with given id: {friend.Id} is not your friend.");

                    user.Friends.Remove(friend);
                    friend.Friends.Remove(user);
                }

                await repositoryWrapper.SaveChanges();
                return NoContent();
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside DeleteFriends method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("main-goal")]
        [SwaggerResponse(StatusCodes.Status200OK, "User's main goal returned", typeof(GoalGetDTO))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> GetUserMainGoal()
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.Goals, null, null),
                    (u => u.Budgets, b => ((Budget)b).Transactions, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                Budget budget = user.Budgets.First(b => b.BudgetType.Equals(BudgetTypeEnum.Personal) ||
                    b.BudgetType.Equals(BudgetTypeEnum.Family));
                Goal? mainGoal = user.Goals.FirstOrDefault(g => g.IsMain);

                GoalGetDTO? mainGoalGetDTO = mainGoal is null ? null : mapper.Map<GoalGetDTO>(mainGoal);
                if (mainGoalGetDTO is not null)
                    GoalManager.CountPercentages(new List<GoalGetDTO> { mainGoalGetDTO }, budget, timeProvider);

                return Ok(mainGoalGetDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside GetUserMainGoal method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("goals")]
        [SwaggerResponse(StatusCodes.Status200OK, "User's goals returned", typeof(List<GoalGetDTO>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> GetUserGoals()
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.Goals, null, null), 
                    (u => u.Budgets, b => ((Budget)b).Transactions, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                Budget budget = user.Budgets.First(b => b.BudgetType.Equals(BudgetTypeEnum.Personal) ||
                    b.BudgetType.Equals(BudgetTypeEnum.Family));

                List<GoalGetDTO> userGoals = mapper.Map<List<GoalGetDTO>>(user.Goals);
                GoalManager.CountPercentages(userGoals, budget, timeProvider);

                return Ok(userGoals);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside GetUserGoals method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
