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
using Org.BouncyCastle.Asn1.X509.Qualified;
using Splitted_backend.Interfaces;
using Splitted_backend.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Security.Claims;
using Models.DTOs.Incoming.Transaction;
using Models.DTOs.Outgoing.Transaction;
using Splitted_backend.Extensions;

namespace Splitted_backend.Controllers
{
    [ApiController]
    [Route("api/transaction")]
    public class TransactionController : ControllerBase
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

        private ILogger<TransactionController> logger { get; }

        private IMapper mapper { get; }

        private IRepositoryWrapper repositoryWrapper { get; }

        private UserManager<User> userManager { get; }


        public TransactionController(ILogger<TransactionController> logger, IMapper mapper, IRepositoryWrapper repositoryWrapper, 
            UserManager<User> userManager)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.repositoryWrapper = repositoryWrapper;
            this.userManager = userManager;
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("csv")]
        [SwaggerResponse(StatusCodes.Status201Created, "Transactions saved")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid bank or file")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> PostCsvTransactions([FromForm] IFormFile csvFile, [FromQuery, BindRequired] BankNameEnum bankName)
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                    return NotFound($"User with id {userId} doesn't exist.");

                if (csvFile.ContentType != "text/csv" || Path.GetExtension(csvFile.FileName) != ".csv")
                    return BadRequest("Received file is not a csv file.");

                BaseCsvReader csvReader;
                Func<IFormFile, BaseCsvReader>? csvReaderFactoryMethod = BankToCsvReaderMapping.GetValueOrDefault(bankName);
                if (csvReaderFactoryMethod is not null)
                    csvReader = csvReaderFactoryMethod(csvFile);
                else
                    return BadRequest("Invalid bank");

                List<TransactionCsv>? transactions = csvReader.GetTransactions(); 
                if (transactions is null || transactions.Count == 0)
                    return BadRequest("Received csv file is invalid or doesn't match the bank.");

                List<Transaction> entityTransactions = mapper.Map<List<Transaction>>(transactions);
                user.Transactions.AddRange(entityTransactions);
                await repositoryWrapper.SaveChanges();

                return CreatedAtAction("PostCsvTransactions", new { count = transactions.Count() });
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside PostCsvTransactions method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [SwaggerResponse(StatusCodes.Status201Created, "Transaction saved")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid body")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> PostTransaction([FromBody] TransactionPostDTO transactionPostDTO)
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

                Transaction transaction = mapper.Map<Transaction>(transactionPostDTO);
                repositoryWrapper.Transactions.Create(transaction);
                user.Transactions.Add(transaction);
                await repositoryWrapper.SaveChanges();

                TransactionCreatedDTO transactionCreatedDTO = mapper.Map<TransactionCreatedDTO>(transaction);
                return CreatedAtAction("PostTransaction", transactionCreatedDTO);
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside PostTransaction method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, "Transactions returned")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> GetUserTransactions()
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdWithIncludesAsync(userId, u => u.Transactions);
                if (user is null)
                    return NotFound($"User with given id: {userId} doesn't exist.");

                List<TransactionGetDTO> userTransactions = mapper.Map<List<TransactionGetDTO>>(user.Transactions); // paging
                return Ok(userTransactions);
                
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside GetUserTransactions method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
