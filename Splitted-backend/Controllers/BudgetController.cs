using AutoMapper;
using CsvConversion.Readers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Models.CsvModels;
using Models.DTOs.Incoming.Budget;
using Models.DTOs.Incoming.Transaction;
using Models.DTOs.Outgoing.Budget;
using Models.DTOs.Outgoing.Transaction;
using Models.Entities;
using Models.Enums;
using Splitted_backend.EntitiesFilters;
using Splitted_backend.Extensions;
using Splitted_backend.Interfaces;
using Splitted_backend.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Splitted_backend.Controllers
{
    [ApiController]
    [Route("api/budgets")]
    public class BudgetController : ControllerBase
    {
        public Dictionary<BankNameEnum, Func<IFormFile, BaseCsvReader>> BankToCsvReaderMapping { get; } =
            new Dictionary<BankNameEnum, Func<IFormFile, BaseCsvReader>>
        {
            { BankNameEnum.Ing, csvFile => new IngCsvReader(csvFile) },
            { BankNameEnum.Mbank, csvFile => new MbankCsvReader(csvFile) },
            { BankNameEnum.Pekao, csvFile => new PekaoCsvReader(csvFile) },
            { BankNameEnum.Santander, csvFile => new SantanderCsvReader(csvFile) },
            { BankNameEnum.Pko, csvFile => new PkoCsvReader(csvFile) },
        };

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
        [SwaggerResponse(StatusCodes.Status201Created, "Budget created", typeof(BudgetCreatedDTO))]
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

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("{budgetId}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Budget updated")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid body")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not a part of the budget")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> PutBudget([FromBody] BudgetPutDTO budgetPutDTO, [FromRoute, BindRequired] Guid budgetId)
        {
            try
            {
                if (budgetPutDTO is null)
                    return BadRequest("BudgetPutDTO object is null.");

                if (!ModelState.IsValid)
                    return BadRequest("Invalid model object.");

                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByCondition(b => b.Id.Equals(budgetId),
                    (b => b.UserBudgets, null));
                if (budget is null)
                    return NotFound($"Budget with id {budgetId} doesn't exist.");

                bool ifBudgetValid = budget.UserBudgets.Any(ub => ub.UserId.Equals(userId));
                if (!ifBudgetValid)
                    return StatusCode(403, $"User with id {userId} isn't a part of the budget with id {budget.Id}");

                mapper.Map(budgetPutDTO, budget);
                repositoryWrapper.Budgets.Update(budget);
                await repositoryWrapper.SaveChanges();

                return NoContent();
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside PutBudget method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("{budgetId}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Budget deleted")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not a part of the budget")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User or budget not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> DeleteBudget([FromRoute, BindRequired] Guid budgetId)
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByCondition(b => b.Id.Equals(budgetId),
                    (b => b.UserBudgets, null));
                if (budget is null)
                    return NotFound($"Budget with id {budgetId} doesn't exist.");

                bool ifBudgetValid = budget.UserBudgets.Any(ub => ub.UserId.Equals(userId));
                if (!ifBudgetValid)
                    return StatusCode(403, $"User with id {userId} isn't a part of the budget with id {budget.Id}");

                repositoryWrapper.Budgets.Delete(budget);
                await repositoryWrapper.SaveChanges();

                return NoContent();
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside DeleteBudget method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("{budgetId}/transactions/csv")]
        [SwaggerResponse(StatusCodes.Status201Created, "Transactions saved", typeof(List<TransactionGetDTO>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid bank or file")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not a part of the budget")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User or budget not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> PostCsvTransactionsToBudget([FromForm] IFormFile csvFile, [FromRoute, BindRequired] Guid budgetId,
            [FromQuery, BindRequired] BankNameEnum bank)
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                    return NotFound($"User with id {userId} doesn't exist.");

                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByCondition(b => b.Id.Equals(budgetId),
                    (b => b.UserBudgets, null), (b => b.Transactions, t => ((Transaction)t).User));
                if (budget is null)
                    return NotFound($"Budget with id {budgetId} doesn't exist.");

                bool ifBudgetValid = budget.UserBudgets.Any(ub => ub.UserId.Equals(userId));
                if (!ifBudgetValid)
                    return StatusCode(403, $"User with id {userId} isn't a part of the budget with id {budget.Id}");

                if (csvFile.ContentType != "text/csv" || Path.GetExtension(csvFile.FileName) != ".csv")
                    return BadRequest("Received file is not a csv file.");

                if (bank.Equals(BankNameEnum.Other))
                    return BadRequest("Loading transactions from a csv file is not supported for this bank.");

                BaseCsvReader csvReader;
                Func<IFormFile, BaseCsvReader>? csvReaderFactoryMethod = BankToCsvReaderMapping.GetValueOrDefault(bank);
                if (csvReaderFactoryMethod is not null)
                    csvReader = csvReaderFactoryMethod(csvFile);
                else
                    return BadRequest("Invalid bank");

                List<TransactionCsv>? transactions = csvReader.GetTransactions();
                if (transactions is null || transactions.Count == 0)
                    return BadRequest("Received csv file is invalid or doesn't match the bank.");

                List<Transaction> entityTransactions = mapper.Map<List<Transaction>>(transactions);

                repositoryWrapper.Transactions.FindDuplicates(entityTransactions, budget.Transactions);
                budget.Transactions.AddRange(entityTransactions);
                user.Transactions.AddRange(entityTransactions);

                decimal balanceDifference = transactions.Aggregate(0M, (prev, current) => prev + current.Amount);
                budget.BudgetBalance += balanceDifference;

                repositoryWrapper.Budgets.Update(budget);
                await repositoryWrapper.SaveChanges();

                List<TransactionGetDTO> transactionGetDTOs = mapper.Map<List<TransactionGetDTO>>(entityTransactions);
                return CreatedAtAction("PostCsvTransactionsToBudget", transactionGetDTOs);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside PostCsvTransactionsToBudget method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("{budgetId}/transactions")]
        [SwaggerResponse(StatusCodes.Status201Created, "Transaction saved", typeof(TransactionCreatedDTO))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid body")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not a part of the budget")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User or budget not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> PostTransactionToBudget([FromBody] TransactionPostDTO transactionPostDTO,
            [FromRoute, BindRequired] Guid budgetId)
        {
            try
            {
                if (transactionPostDTO is null)
                    return BadRequest("TransactionPostDTO object is null.");

                if (!ModelState.IsValid)
                    return BadRequest("Invalid model object.");

                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                    return NotFound($"User with id {userId} doesn't exist.");

                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByCondition(b => b.Id.Equals(budgetId),
                    (b => b.UserBudgets, null), (b => b.Transactions, null));
                if (budget is null)
                    return NotFound($"Budget with id {budgetId} doesn't exist.");

                bool ifBudgetValid = budget.UserBudgets.Any(ub => ub.UserId.Equals(userId));
                if (!ifBudgetValid)
                    return StatusCode(403, $"User with id {userId} isn't a part of the budget with id {budget.Id}");

                Transaction transaction = mapper.Map<Transaction>(transactionPostDTO);

                repositoryWrapper.Transactions.FindDuplicates(new List<Transaction> { transaction }, budget.Transactions);
                budget.Transactions.Add(transaction);
                user.Transactions.Add(transaction);

                budget.BudgetBalance += transaction.Amount;
                repositoryWrapper.Budgets.Update(budget);
                await repositoryWrapper.SaveChanges();

                TransactionCreatedDTO transactionCreatedDTO = mapper.Map<TransactionCreatedDTO>(transaction);
                return CreatedAtAction("PostTransactionToBudget", transactionCreatedDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside PostTransactionToBudget method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{budgetId}/transactions")]
        [SwaggerResponse(StatusCodes.Status200OK, "Transactions returned", typeof(List<TransactionGetDTO>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not a part of the budget")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User or budget not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> GetBudgetTransactions([FromRoute, BindRequired] Guid budgetId, [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo, [FromQuery] decimal? minAmount, [FromQuery] decimal? maxAmount, [FromQuery] string? category,
            [FromQuery] string? userName)
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                    return NotFound($"User with id {userId} doesn't exist.");

                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByCondition(b => b.Id.Equals(budgetId),
                    (b => b.Transactions, t => ((Transaction)t).User), (b => b.UserBudgets, null));
                if (budget is null)
                    return NotFound($"Budget with id {budgetId} doesn't exist.");

                bool ifBudgetValid = budget.UserBudgets.Any(ub => ub.UserId.Equals(userId));
                if (!ifBudgetValid)
                    return StatusCode(403, $"User with id {userId} isn't a part of the budget with id {budget.Id}");

                TransactionsFilter transactionsFilter = new TransactionsFilter(
                    dates: (dateFrom, dateTo),
                    amounts: (minAmount, maxAmount),
                    category: category,
                    userName: userName,
                    userManager: userManager
                );
                List<Transaction> transactionsFiltered = await transactionsFilter.GetFilteredTransactions(budget.Transactions);
                List<TransactionGetDTO> transactionsFilteredDTO = mapper.Map<List<TransactionGetDTO>>(transactionsFiltered);

                return Ok(transactionsFilteredDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside GetBudgetTransactions method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

    }
}
