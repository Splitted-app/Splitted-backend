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
        [SwaggerResponse(StatusCodes.Status200OK, "Transactions saved")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid bank or file")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> PostCsvTransactions([FromForm] IFormFile csvFile, [FromQuery, BindRequired] BankNameEnum bankName)
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                    return BadRequest($"User with id {userId} doesn't exist.");

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

                List<Transaction> entityTransactions = mapper.Map<List<TransactionCsv>, List<Transaction>>(transactions);
                user.Transactions.AddRange(entityTransactions);
                await repositoryWrapper.SaveChanges();

                return Ok();
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside PostCsvTransactions method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
