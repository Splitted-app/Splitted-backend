using AutoMapper;
using CsvConversion.Readers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Models.CsvModels;
using Models.Entities;
using Splitted_backend.Interfaces;
using Splitted_backend.Models.Entities;
using System.Collections.Generic;

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


        [HttpPost("csv")]
        public async Task<IActionResult> PostCsvTransactions([FromQuery, BindRequired] Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest("UserId is empty.");

                User? user = await userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                    return BadRequest($"User with id {userId} doesn't exist.");

                string path = "C:\\Users\\Mateusz\\Desktop\\Programowanko\\Praca inżynierska\\CSvki\\Pekao.csv";
                BaseCsvReader reader = new PekaoCsvReader(path);
                var transactions = reader.GetTransactions();
                var entityTransactions = mapper.Map<List<TransactionCsv>, IEnumerable<Transaction>>(transactions);

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
