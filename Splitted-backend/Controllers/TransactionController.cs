using AutoMapper;
using CsvConversion.Readers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Models.CsvModels;
using Models.Entities;
using Models.Enums;
using Splitted_backend.Interfaces;
using Splitted_backend.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Security.Claims;
using Models.DTOs.Incoming.Transaction;
using Models.DTOs.Outgoing.Transaction;
using Splitted_backend.Extensions;
using AIService;

namespace Splitted_backend.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionController : ControllerBase
    {
        private ILogger<TransactionController> logger { get; }

        private IMapper mapper { get; }

        private IRepositoryWrapper repositoryWrapper { get; }

        private UserManager<User> userManager { get; }

        private PythonExecuter pythonExecuter { get; }


        public TransactionController(ILogger<TransactionController> logger, IMapper mapper, IRepositoryWrapper repositoryWrapper, 
            UserManager<User> userManager, PythonExecuter pythonExecuter)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.repositoryWrapper = repositoryWrapper;
            this.userManager = userManager;
            this.pythonExecuter = pythonExecuter;
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("{transactionId}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Transaction updated")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid body")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Transaction doesn't belong to the user")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Transaction or user not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> PutTransaction([FromRoute, BindRequired] Guid transactionId, 
            [FromBody] TransactionPutDTO transactionPutDTO)
        {
            try
            {
                if (transactionPutDTO is null)
                    return BadRequest("TransactionPutDTO object is null.");

                if (!ModelState.IsValid)
                    return BadRequest("Invalid model object.");

                Transaction? transaction = await repositoryWrapper.Transactions.GetEntityOrDefaultByCondition(t => t.Id.Equals(transactionId));
                if (transaction is null)
                    return NotFound($"Transaction with given id: {transactionId} doesn't exist.");

                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.UserBudgets, null), (u => u.Transactions, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                Guid budgetId = transaction.BudgetId;
                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByCondition(b => b.Id.Equals(budgetId), 
                    (b => b.Transactions, null));
                if (budget is null)
                    return NotFound($"Budget with id {budgetId} doesn't exist.");

                bool ifTransactionValid = user.Transactions.Any(t => t.Id.Equals(transactionId));
                if (!ifTransactionValid)
                    return StatusCode(403, $"Transaction doesn't belong to the user with id: {userId}.");
                
                if (transactionPutDTO.Amount is not null)
                {
                    decimal balanceDifference = (decimal)transactionPutDTO.Amount - transaction.Amount;
                    budget.BudgetBalance += balanceDifference;
                }

                mapper.Map(transactionPutDTO, transaction);
                repositoryWrapper.Transactions.FindDuplicates(new List<Transaction> { transaction }, budget.Transactions);
                repositoryWrapper.Transactions.Update(transaction);
                repositoryWrapper.Budgets.Update(budget);
                await repositoryWrapper.SaveChanges();

                return NoContent();
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside PutTransaction method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("{*transactionIds}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Transactions deleted")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid path parameter")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Transaction doesn't belong to user")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Transaction or user not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> DeleteTransactions([FromRoute, BindRequired] string transactionIds)
        {
            try
            {
                List<Guid> transactionIdsList = new List<Guid>();
                List<string> transactionIdsStrings = transactionIds.Split("/")
                    .ToList();

                foreach (string transactionIdString in transactionIdsStrings)
                {
                    Guid transactionId;
                    bool parsed = Guid.TryParse(transactionIdString, out transactionId);
                    if (parsed)
                        transactionIdsList.Add(transactionId);
                    else
                        return BadRequest("Some of transactionIds is invalid.");
                }

                List<Transaction> transactions = await repositoryWrapper.Transactions.GetEntitiesByCondition(t => transactionIdsList.Contains(t.Id));
                if (transactionIdsList.Count != transactions.Count)
                    return NotFound("Some of transactions were not found.");

                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.UserBudgets, null), (u => u.Transactions, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                Guid budgetId = transactions[0].BudgetId;
                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByCondition(b => b.Id.Equals(budgetId));
                if (budget is null)
                    return NotFound($"Budget with id {budgetId} doesn't exist.");

                bool ifBudgetValid = user.UserBudgets.Any(ub => ub.BudgetId.Equals(budgetId));
                if (!ifBudgetValid)
                    return StatusCode(403, $"User with id {userId} isn't a part of the budget with id {budget.Id}");

                bool ifTransactionsValid = transactions.All(t => user.Transactions.Any(ut => ut.Id.Equals(t.Id)));
                if (!ifTransactionsValid)
                    return StatusCode(403, $"Some of transactions don't belong to the user with id: {userId}.");

                decimal balanceDifference = transactions.Aggregate(0M, (prev, current) => prev + current.Amount);
                budget.BudgetBalance -= balanceDifference;

                repositoryWrapper.Transactions.DeleteMultiple(transactions);
                repositoryWrapper.Budgets.Update(budget);
                await repositoryWrapper.SaveChanges();

                return NoContent();
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside DeleteTransactions method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("ai-train")]
        [SwaggerResponse(StatusCodes.Status200OK, "Training suceeded")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Email not confirmed")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> TrainAIModel()
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.Transactions, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                List<TransactionAITrainDTO> transactionsAITrainDTO = mapper.Map<List<TransactionAITrainDTO>>(user.Transactions);
                pythonExecuter.TrainAIModel(transactionsAITrainDTO);

                return Ok();
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside TrainAIModel method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

    }
}
