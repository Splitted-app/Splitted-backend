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
using Splitted_backend.Managers;
using Models.DTOs.Outgoing.Insights;
using System;

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
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not a part of the budget")]
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

                Transaction? transaction = await repositoryWrapper.Transactions.GetEntityOrDefaultByConditionAsync(t => t.Id.Equals(transactionId));
                if (transaction is null)
                    return NotFound($"Transaction with given id: {transactionId} doesn't exist.");

                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.UserBudgets, null, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                Guid budgetId = transaction.BudgetId;
                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByConditionAsync(b => b.Id.Equals(budgetId), 
                    (b => b.Transactions, null, null));
                if (budget is null)
                    return NotFound($"Budget with id {budgetId} doesn't exist.");

                bool ifBudgetValid = user.UserBudgets.Any(ub => ub.BudgetId.Equals(budgetId));
                if (!ifBudgetValid)
                    return StatusCode(403, $"User with id {userId} isn't a part of the budget with id {budget.Id}");
                
                if (transactionPutDTO.Amount is not null && transaction.Date >= budget.CreationDate)
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
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not a part of the budget")]
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
                        return BadRequest("Some of transactionIds are invalid.");
                }

                List<Transaction> transactions = await repositoryWrapper.Transactions.GetEntitiesByConditionAsync(t => transactionIdsList.Contains(t.Id),
                    (t => t.DuplicatedTransactions, null, null), 
                    (t => t.TransactionPayBacksResolved, null, null));
                if (transactionIdsList.Count != transactions.Count)
                    return NotFound("Some of transactions were not found.");

                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.Budgets, null, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                Guid budgetId = transactions[0].BudgetId;
                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByConditionAsync(b => b.Id.Equals(budgetId));
                if (budget is null)
                    return NotFound($"Budget with id {budgetId} doesn't exist.");

                bool ifBudgetValid = user.Budgets.Any(b => b.Id.Equals(budgetId));
                if (!ifBudgetValid)
                    return StatusCode(403, $"User with id {userId} isn't a part of the budget with id {budget.Id}");

                InsightsIncomeExpensesDTO incomeExpensesDTO = InsightsManager.GetIncomeExpenses(transactions
                    .Where(t => t.Date >= budget.CreationDate)
                    .ToList());
                budget.BudgetBalance -= (incomeExpensesDTO.Expenses + incomeExpensesDTO.Income);

                transactions.ForEach(t => t.TransactionPayBacksResolved.ForEach(tpb => tpb.PayBackTransactionId = null));
                transactions.ForEach(t => t.DuplicatedTransactions.ForEach(dt => dt.DuplicatedTransactionId = null));

                budget.Transactions.RemoveAll(bt => transactions.Any(t => t.Id.Equals(bt.Id)));
                repositoryWrapper.Transactions.FindDuplicates(budget.Transactions, budget.Transactions);

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
        [HttpPut("{paybackTransactionId?}/payback/{*transactionIds}")]
        [HttpPut("null/payback/{*transactionIds}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Payback updated")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid path parameter")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not a part of the budget or is not allowed to pay back")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Transaction or user not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> PayTransactionsBack([FromRoute, BindRequired] string transactionIds,
            [FromRoute] Guid? paybackTransactionId)
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
                        return BadRequest("Some of transactionIds are invalid.");
                }

                List<Transaction> transactions = await repositoryWrapper.Transactions.GetEntitiesByConditionAsync(t => transactionIdsList.Contains(t.Id),
                    (t => t.TransactionPayBacks, null, null));
                if (transactionIdsList.Count != transactions.Count)
                    return NotFound("Some of transactions were not found.");

                if (paybackTransactionId is not null)
                {
                    Transaction? paybackTransaction = await repositoryWrapper.Transactions
                        .GetEntityOrDefaultByConditionAsync(t => t.Id.Equals(paybackTransactionId),
                        (t => t.Budget, null, null));
                    if (paybackTransaction is null)
                        return NotFound($"Payback transaction with given id: {paybackTransactionId} doesn't exist.");

                    if (paybackTransaction.Budget.BudgetType.Equals(BudgetTypeEnum.Partner)
                    || paybackTransaction.Budget.BudgetType.Equals(BudgetTypeEnum.Temporary))
                        return StatusCode(403, "Payback transaction cannot come from partner or temporary budget.");
                }
        
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.UserBudgets, null, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                if (transactions.Any(t => t.TransactionPayBacks.Any(tpb => tpb.OwedUserId.Equals(userId))))
                    return StatusCode(403, "You are not allowed to pay yourself back.");

                Guid budgetId = transactions[0].BudgetId;
                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByConditionAsync(b => b.Id.Equals(budgetId),
                    (b => b.Transactions, null, null));
                if (budget is null)
                    return NotFound($"Budget with id {budgetId} doesn't exist.");

                bool ifBudgetValid = user.UserBudgets.Any(ub => ub.BudgetId.Equals(budgetId));
                if (!ifBudgetValid)
                    return StatusCode(403, $"User with id {userId} isn't a part of the budget with id {budget.Id}");

                transactions.ForEach(t => ModeManager.MakePayback(t, paybackTransactionId, userId));
                await repositoryWrapper.SaveChanges();

                return NoContent();
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside PayTransactionsBack method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("{transactionId}/resolve-payback/{transactionPayBackId}")]
        [SwaggerResponse(StatusCodes.Status201Created, "Payback resolved")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid query parameter")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not a part of the budget or is not allowed to resolve payback")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Transaction, transactionPayBack or user not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> ResolvePayback([FromRoute, BindRequired] Guid transactionId,
            [FromRoute, BindRequired] Guid transactionPayBackId, [FromQuery, BindRequired] bool accept)
        {
            try
            {
                Transaction? transaction = await repositoryWrapper.Transactions
                    .GetEntityOrDefaultByConditionAsync(t => t.Id.Equals(transactionId), (t => t.TransactionPayBacks, null, null));
                if (transaction is null)
                    return NotFound($"Transaction with given id: {transactionId} doesn't exist.");

                if (!transaction.TransactionPayBacks.Any(tbd => tbd.Id.Equals(transactionPayBackId)))
                    return NotFound($"TransactionPayBack with given id: {transactionPayBackId} doesn't exist.");

                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.UserBudgets, null, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                if (transaction.TransactionPayBacks.Any(tpb => tpb.OwingUserId.Equals(userId)))
                    return StatusCode(403, "You are not allowed to resolve payback.");

                Guid budgetId = transaction.BudgetId;
                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByConditionAsync(b => b.Id.Equals(budgetId),
                    (b => b.Transactions, null, null));
                if (budget is null)
                    return NotFound($"Budget with id {budgetId} doesn't exist.");

                bool ifBudgetValid = user.UserBudgets.Any(ub => ub.BudgetId.Equals(budgetId));
                if (!ifBudgetValid)
                    return StatusCode(403, $"User with id {userId} isn't a part of the budget with id {budget.Id}");

                if (!transaction.TransactionPayBacks.First(tpb => tpb.Id.Equals(transactionPayBackId))
                    .TransactionPayBackStatus.Equals(TransactionPayBackStatusEnum.WaitingForApproval))
                    return StatusCode(403, "TransactionPayBack is not paid back yet.");

                ModeManager.ResolvePayback(transaction, transactionPayBackId, accept);
                repositoryWrapper.Transactions.Update(transaction);

                if (transaction.TransactionPayBacks.All(tpb => tpb.TransactionPayBackStatus
                    .Equals(TransactionPayBackStatusEnum.PaidBack)))
                {
                    repositoryWrapper.Transactions.Delete(transaction);
                }
                await repositoryWrapper.SaveChanges();

                return NoContent();
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside ResolvePayback method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("train-ai")]
        [SwaggerResponse(StatusCodes.Status200OK, "Training suceeded")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Not enough transactions")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Email not confirmed")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> TrainAIModel()
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.Transactions, null, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                if (!user.EmailConfirmed)
                    return StatusCode(403, "You are not allowed to train AI model with your email not cofirmed.");

                if (user.Transactions.Count == 0)
                    return BadRequest("To train AI model you must have at least 1 transaction.");

                List<TransactionAITrainDTO> transactionsAITrainDTO = mapper.Map<List<TransactionAITrainDTO>>(user.Transactions);
                pythonExecuter.TrainModel(transactionsAITrainDTO, userId.ToString());

                return Ok();
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside TrainModel method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

    }
}
