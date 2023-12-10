using AIService;
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
using Models.DTOs.Outgoing.Insights;
using Models.DTOs.Outgoing.Transaction;
using Models.Entities;
using Models.Enums;
using Splitted_backend.EntitiesFilters;
using Splitted_backend.Extensions;
using Splitted_backend.Interfaces;
using Splitted_backend.Managers;
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

        private PythonExecuter pythonExecuter { get; }


        public BudgetController(ILogger<BudgetController> logger, IMapper mapper, IRepositoryWrapper repositoryWrapper, 
            UserManager<User> userManager, PythonExecuter pythonExecuter)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.repositoryWrapper = repositoryWrapper;
            this.userManager = userManager;
            this.pythonExecuter = pythonExecuter;
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{budgetId}")]
        [SwaggerResponse(StatusCodes.Status200OK, "Budget info returned", typeof(BudgetGetDTO))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not a part of the budget")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Budget or user not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> GetBudget([FromRoute, BindRequired] Guid budgetId)
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByConditionAsync(b => b.Id.Equals(budgetId),
                    (b => b.Users, null, null));
                if (budget is null)
                    return NotFound($"Budget with id {budgetId} doesn't exist.");

                bool ifBudgetValid = budget.Users.Any(ub => ub.Id.Equals(userId));
                if (!ifBudgetValid)
                    return StatusCode(403, $"User with id {userId} isn't a part of the budget with id {budget.Id}");

                BudgetGetDTO budgetGetDTO = mapper.Map<BudgetGetDTO>(budget);
                return Ok(budgetGetDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside GetBudget method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [SwaggerResponse(StatusCodes.Status201Created, "Budget created", typeof(BudgetCreatedDTO))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid body")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Invalid budget type")]
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
                User? user = await userManager.FindByIdWithIncludesAsync(userId, (u => u.Budgets, null, null));
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                if (user.Budgets.Any(b => b.BudgetType.Equals(BudgetTypeEnum.Personal) || b.BudgetType.Equals(BudgetTypeEnum.Family)))
                    return StatusCode(403, "User cannot have more than one personal or family budget.");

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
        public async Task<IActionResult> PutBudget([FromBody] BudgetPutDTO budgetPutDTO, 
            [FromRoute, BindRequired] Guid budgetId)
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

                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByConditionAsync(b => b.Id.Equals(budgetId),
                    (b => b.UserBudgets, null, null));
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
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not a part of the budget or invalid budget type")]
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

                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByConditionAsync(b => b.Id.Equals(budgetId),
                    (b => b.UserBudgets, null, null));
                if (budget is null)
                    return NotFound($"Budget with id {budgetId} doesn't exist.");

                bool ifBudgetValid = budget.UserBudgets.Any(ub => ub.UserId.Equals(userId));
                if (!ifBudgetValid)
                    return StatusCode(403, $"User with id {userId} isn't a part of the budget with id {budget.Id}");

                if (budget.BudgetType.Equals(BudgetTypeEnum.Personal) || budget.BudgetType.Equals(BudgetTypeEnum.Family))
                    return StatusCode(403, "Wrong type of budget.");

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
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not a part of the budget or invalid budget type")]
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

                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByConditionAsync(b => b.Id.Equals(budgetId),
                    (b => b.UserBudgets, null, null), (b => b.Transactions, t => ((Transaction)t).User, null));
                if (budget is null)
                    return NotFound($"Budget with id {budgetId} doesn't exist.");

                bool ifBudgetValid = budget.UserBudgets.Any(ub => ub.UserId.Equals(userId));
                if (!ifBudgetValid)
                    return StatusCode(403, $"User with id {userId} isn't a part of the budget with id {budget.Id}");

                if (budget.BudgetType.Equals(BudgetTypeEnum.Partner) || budget.BudgetType.Equals(BudgetTypeEnum.Temporary))
                    return StatusCode(403, "You cannot add new transactions to partner or temporary budget.");

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

                List<TransactionCsv>? importedTransactions = csvReader.GetTransactions();
                if (importedTransactions is null || importedTransactions.Count == 0)
                    return BadRequest("Received csv file is invalid or doesn't match the bank.");

                List<TransactionAITrainDTO> importedAiTransactions = mapper.Map<List<TransactionAITrainDTO>>(importedTransactions);
                pythonExecuter.CategorizeTransactions(importedTransactions, importedAiTransactions, userId.ToString());

                List<Transaction> entityTransactions = mapper.Map<List<Transaction>>(importedTransactions);

                repositoryWrapper.Transactions.FindDuplicates(entityTransactions, budget.Transactions);
                budget.Transactions.AddRange(entityTransactions);
                user.Transactions.AddRange(entityTransactions);

                InsightsIncomeExpensesDTO incomeExpensesDTO = InsightsManager.GetIncomeExpenses(entityTransactions
                    .Where(t => t.Date >= budget.CreationDate)
                    .ToList());
                budget.BudgetBalance += (incomeExpensesDTO.Expenses + incomeExpensesDTO.Income);

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
        [SwaggerResponse(StatusCodes.Status201Created, "Transaction saved", typeof(TransactionGetDTO))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid body")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not a part of the budget or invalid budget type")]
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

                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByConditionAsync(b => b.Id.Equals(budgetId),
                    (b => b.UserBudgets, null, null), (b => b.Transactions, null, null));
                if (budget is null)
                    return NotFound($"Budget with id {budgetId} doesn't exist.");

                bool ifBudgetValid = budget.UserBudgets.Any(ub => ub.UserId.Equals(userId));
                if (!ifBudgetValid)
                    return StatusCode(403, $"User with id {userId} isn't a part of the budget with id {budget.Id}");

                if (budget.BudgetType.Equals(BudgetTypeEnum.Partner) || budget.BudgetType.Equals(BudgetTypeEnum.Temporary))
                    return StatusCode(403, "You cannot add new transactions to partner or temporary budget.");

                Transaction transaction = mapper.Map<Transaction>(transactionPostDTO);

                repositoryWrapper.Transactions.FindDuplicates(new List<Transaction> { transaction }, budget.Transactions);
                budget.Transactions.Add(transaction);
                user.Transactions.Add(transaction);

                if (transaction.Date >= budget.CreationDate)
                    budget.BudgetBalance += transaction.Amount;

                repositoryWrapper.Budgets.Update(budget);
                await repositoryWrapper.SaveChanges();

                TransactionGetDTO transactionCreatedDTO = mapper.Map<TransactionGetDTO>(transaction);
                return CreatedAtAction("PostTransactionToBudget", transactionCreatedDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside PostTransactionToBudget method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("{budgetId}/transactions/{*transactionIds}")]
        [SwaggerResponse(StatusCodes.Status201Created, "Transaction saved", typeof(TransactionGetDTO))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid path parameter")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not a part of the budget or invalid budget type")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User, budget or transactions not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> PostExistingTransactionsToBudget([FromRoute, BindRequired] Guid budgetId,
            [FromRoute, BindRequired] string transactionIds)
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                    return NotFound($"User with id {userId} doesn't exist.");

                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByConditionAsync(b => b.Id.Equals(budgetId),
                    (b => b.UserBudgets, null, null), (b => b.Transactions, null, null));
                if (budget is null)
                    return NotFound($"Budget with id {budgetId} doesn't exist.");

                bool ifBudgetValid = budget.UserBudgets.Any(ub => ub.UserId.Equals(userId));
                if (!ifBudgetValid)
                    return StatusCode(403, $"User with id {userId} isn't a part of the budget with id {budget.Id}");

                if (budget.BudgetType.Equals(BudgetTypeEnum.Personal) || budget.BudgetType.Equals(BudgetTypeEnum.Family))
                    return StatusCode(403, "You cannot add existing transactions to personal or family budget.");

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

                List<Transaction> originalTransactions = await repositoryWrapper.Transactions
                    .GetEntitiesByConditionAsync(t => transactionIdsList.Contains(t.Id));
                if (transactionIdsList.Count != originalTransactions.Count)
                    return NotFound("Some of transactions were not found.");

                bool ifTransactionsValid = originalTransactions.All(t => user.Transactions.Any(ut => ut.Id.Equals(t.Id)));
                if (!ifTransactionsValid)
                    return StatusCode(403, $"Some of transactions don't belong to the user with id: {userId}.");

                List<Transaction> transactionsToAdd = originalTransactions
                    .Select(t => t.Copy())
                    .ToList();

                List<Guid> otherUserIds = budget.UserBudgets
                    .Where(ub => !ub.UserId.Equals(userId))
                    .Select(ub => ub.UserId)
                    .ToList();

                repositoryWrapper.Transactions.FindDuplicates(transactionsToAdd, budget.Transactions);
                ModeManager.DeterminePayBacks(transactionsToAdd, userId, otherUserIds);
                budget.Transactions.AddRange(transactionsToAdd);

                repositoryWrapper.Budgets.Update(budget);
                await repositoryWrapper.SaveChanges();

                List<TransactionGetDTO> transactionsCreatedDTO = mapper.Map<List<TransactionGetDTO>>(transactionsToAdd);
                return CreatedAtAction("PostExistingTransactionsToBudget", transactionsCreatedDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside PostExistingTransactionsToBudget method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{budgetId}/transactions")]
        [SwaggerResponse(StatusCodes.Status200OK, "Transactions returned", typeof(BudgetTransactionsGetDTO))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid query parameter")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not a part of the budget or invalid budget type")]
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

                Budget? budget = await repositoryWrapper.Budgets.GetEntityOrDefaultByConditionAsync(b => b.Id.Equals(budgetId),
                    (b => b.Transactions, t => ((Transaction)t).User, null),
                    (b => b.UserBudgets, null, null),
                    (b => b.Transactions, t => ((Transaction)t).TransactionPayBacks, 
                    tpb => ((TransactionPayBack)tpb).OwingUser));
                if (budget is null)
                    return NotFound($"Budget with id {budgetId} doesn't exist.");

                bool ifBudgetValid = budget.UserBudgets.Any(ub => ub.UserId.Equals(userId));
                if (!ifBudgetValid)
                    return StatusCode(403, $"User with id {userId} isn't a part of the budget with id {budget.Id}");

                TransactionsFilter transactionsFilter = new TransactionsFilter (
                    dates: (dateFrom, dateTo),
                    amounts: (minAmount, maxAmount),
                    category: category,
                    userName: userName
                );
                List<Transaction> transactionsFiltered = transactionsFilter.GetFilteredTransactions(budget.Transactions);

                decimal debt = (budget.BudgetType.Equals(BudgetTypeEnum.Personal)
                    || budget.BudgetType.Equals(BudgetTypeEnum.Family)) ? 0
                    : ModeManager.GetUserDebt(budget, userId, budget.UserBudgets.Count());
                InsightsIncomeExpensesDTO incomeExpensesDTO = InsightsManager.GetIncomeExpenses(transactionsFiltered);

                foreach (Transaction transactionFiltered in transactionsFiltered)
                {
                    transactionFiltered.TransactionPayBacks = transactionFiltered.TransactionPayBacks
                        .Where(tpb => tpb.OwedUserId.Equals(userId))
                        .ToList();
                }

                List<TransactionGetDTO> transactionsFilteredDTO = mapper.Map<List<TransactionGetDTO>>(transactionsFiltered);
                BudgetTransactionsGetDTO budgetTransactionsGetDTO = new BudgetTransactionsGetDTO
                {
                    Transactions = transactionsFilteredDTO,
                    Income = incomeExpensesDTO.Income,
                    Expenses = incomeExpensesDTO.Expenses,
                    Debt = debt,
                };
                return Ok(budgetTransactionsGetDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside GetBudgetTransactions method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

    }
}
