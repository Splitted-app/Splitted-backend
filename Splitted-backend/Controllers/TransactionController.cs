using AutoMapper;
using CsvConversion.Readers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Models.CsvModels;
using Models.Entities;
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
        public async Task<IActionResult> PostCsvTransactions([FromForm] IFormFile csvFile, [FromQuery] string bank)
        {
            try
            {
                Guid userId = new Guid(User.FindFirstValue("user_id"));
                User? user = await userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                    return BadRequest($"User with id {userId} doesn't exist.");

                if (csvFile.ContentType != "text/csv" || Path.GetExtension(csvFile.FileName) != ".csv") // test jak sie wysle plik spreparowany tj. na chama .csv
                    return BadRequest("Received file is not a csv file.");

                BaseCsvReader csvReader; // ten switch tez do refactoringu najlepiej (bank na enum tez)
                switch (bank.ToLower())
                {
                    case "pko":
                        csvReader = new PkoCsvReader(csvFile);
                        break;
                    case "santander":
                        csvReader = new SantanderCsvReader(csvFile);
                        break;
                    case "pekao":
                        csvReader = new PekaoCsvReader(csvFile);
                        break;
                    case "ing":
                        csvReader = new IngCsvReader(csvFile);
                        break;
                    case "mbank":
                        csvReader = new MbankCsvReader(csvFile);
                        break;
                    default:
                        return BadRequest("Invalid bank.");
                }

                List<TransactionCsv> transactions = csvReader.GetTransactions(); // mozna zmodyfikowac by zwracac czy czytanie się powiodło (try catch na
                // mapowanie wiersza na model - wtedy oznacza ze wgrany plik nie jest z dobrego banku) - jesli cos sie wywali w innym miejscu to juz nieprzewidziane
                // i 500
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
