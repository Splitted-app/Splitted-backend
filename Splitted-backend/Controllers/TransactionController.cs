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


        public TransactionController(ILogger<TransactionController> logger, IMapper mapper, IRepositoryWrapper repositoryWrapper, 
            UserManager<User> userManager)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.repositoryWrapper = repositoryWrapper;
            this.userManager = userManager;
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("{transactionId}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Transaction updated")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid query parameter or body")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Transaction doesn't belong to the user")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Transaction or user not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> PutTransaction([FromRoute, BindRequired] Guid transactionId, 
            [FromBody] TransactionPutDTO transactionPutDTO)
        {
            try
            {
                if (transactionId.Equals(Guid.Empty))
                    return BadRequest("TransactionId is empty.");

                if (transactionPutDTO is null)
                    return BadRequest("TransactionPutDTO object is null.");

                if (!ModelState.IsValid)
                    return BadRequest("Invalid model object.");

                Transaction? transaction = await repositoryWrapper.Transactions.GetEntityOrDefaultByCondition(t => t.Id.Equals(transactionId));

                if (transaction is null)
                    return NotFound($"Transaction with given id: {transactionId} doesn't exist.");

                //Guid userId = new Guid(User.FindFirstValue("user_id"));
                //User? user = await userManager.FindByIdWithIncludesAsync(userId, u => u.Transactions);
                //if (user is null)
                //    return NotFound($"User with given id: {userId} doesn't exist.");

                //bool ifTransactionValid = user.Transactions.Any(t => t.Id.Equals(transactionId));
                //if (!ifTransactionValid)
                //    return Forbid($"Transaction doesn't belong to the user with id: {userId}.");

                //mapper.Map(transactionPutDTO, transaction);
                //repositoryWrapper.Transactions.Update(transaction);
                //await repositoryWrapper.SaveChanges();

                return NoContent();
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside PutTransaction method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("{transactionIds}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Transactions deleted")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid query parameter")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized to perform the action")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Transaction doesn't belong to user")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Transaction or user not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> DeleteTransactions([FromRoute, BindRequired] string transactionIds)
        {
            try
            {
                if (transactionIds is null)
                    return BadRequest("Query parameter is null.");

                List<Guid> transactionIdsList = new List<Guid>();
                List<string> transactionIdStrings = transactionIds.Split(",")
                    .ToList();

                foreach (string transactionIdString in transactionIdStrings)
                {
                    Guid transactionId;
                    bool parsed = Guid.TryParse(transactionIdString, out transactionId);
                    if (parsed)
                        transactionIdsList.Add(transactionId);
                    else
                        return BadRequest("One of transactionIds is invalid.");
                }

                if (transactionIdsList.Any(ti => ti.Equals(Guid.Empty)))
                    return BadRequest("One of transactionIds is empty.");

                List<Transaction> transactions = await repositoryWrapper.Transactions.GetEntitiesByCondition(t => transactionIdsList.Contains(t.Id));
                if (transactionIdsList.Count != transactions.Count)
                    return NotFound("Some of the transaction were not found.");

                //Guid userId = new Guid(User.FindFirstValue("user_id"));
                //User? user = await userManager.FindByIdWithIncludesAsync(userId, u => u.Transactions);
                //if (user is null)
                //    return NotFound($"User with given id: {userId} doesn't exist.");

                //bool ifTransactionsValid = transactions.All(t => user.Transactions.Contains(t));
                //if (!ifTransactionsValid)
                //    return Forbid($"Transaction doesn't belong to the user with id: {userId}.");

                //repositoryWrapper.Transactions.DeleteMultiple(transactions);
                //await repositoryWrapper.SaveChanges();

                return NoContent();
            }
            catch (Exception exception)
            {
                logger.LogError($"Error occurred inside DeleteTransaction method. {exception}.");
                return StatusCode(500, "Internal server error.");
            }
        }

    }
}
