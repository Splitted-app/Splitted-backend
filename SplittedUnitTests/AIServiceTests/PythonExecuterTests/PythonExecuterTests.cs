using AIService;
using FluentAssertions;
using Models.CsvModels;
using Models.DTOs.Outgoing.Transaction;
using SplittedUnitTests.AIServiceTests.Fixtures;
using SplittedUnitTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.AIServiceTests.PythonExecuterTests
{
    public class PythonExecuterTests : IClassFixture<PythonExecuterTestFixture>
    {
        private PythonExecuterTestFixture pythonExecuterTestFixture { get; }


        public PythonExecuterTests(PythonExecuterTestFixture pythonExecuterTestFixture)
        {
            this.pythonExecuterTestFixture = pythonExecuterTestFixture;
        }


        [Fact]
        public void Test_TrainAIModel()
        {
            Guid userId = Guid.NewGuid();
            string expectedFilePath = "../../../Data/FakeAIServiceData/model.pickle";

            Action action = () =>
            {
                pythonExecuterTestFixture.pythonExecuter.TrainModel(new List<TransactionAITrainDTO>(),
                userId.ToString());
            };

            action.Should().NotThrow();
            File.Exists(expectedFilePath).Should().BeTrue();

            string fileContent = File.ReadAllText(expectedFilePath);
            fileContent.Should().Be(userId.ToString());

            File.Delete(expectedFilePath);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("1279ebd9-c55d-4436-a2e2-62fffb1faad0")]
        public void Test_CategorizeTransactions(string userId)
        {
            List<TransactionCsv> importedTransactions = new List<TransactionCsv>
            {
                new TransactionCsv
                {
                    Amount = 124,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-23"),
                    Description = "Transaction 1",
                    TransactionType = Models.Enums.TransactionTypeEnum.Other,
                },

                new TransactionCsv
                {
                    Amount = -45.65M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-24"),
                    Description = "Transaction 2",
                    TransactionType = Models.Enums.TransactionTypeEnum.Card,
                    BankCategory = "Biedronka",
                },

                new TransactionCsv
                {
                    Amount = -50,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-24"),
                    Description = "Transaction 3",
                    TransactionType = Models.Enums.TransactionTypeEnum.Card,
                },

                new TransactionCsv
                {
                    Amount = -200,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-25"),
                    Description = "Transaction 4",
                    TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                    BankCategory = "Auchan",
                },

                new TransactionCsv
                {
                    Amount = -89.99M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-26"),
                    Description = "Transaction 5",
                    TransactionType = Models.Enums.TransactionTypeEnum.Blik,
                    BankCategory = "Żabka",
                },

                new TransactionCsv
                {
                    Amount = 124,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-27"),
                    Description = "Transaction 6",
                    TransactionType = Models.Enums.TransactionTypeEnum.Other,
                },

                new TransactionCsv
                {
                    Amount = 12000,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-28"),
                    Description = "Transaction 7",
                    TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                    BankCategory = "Praca",
                }
            };

            List<TransactionAITrainDTO> importedAiTransactions = importedTransactions
                .Select(it => new TransactionAITrainDTO
                {
                    Date = it.Date.ToString(),
                    Description = it.Description,
                    Amount = it.Amount,
                    Currency = it.Currency,
                    BankCategory = it.BankCategory
                })
                .ToList();

            pythonExecuterTestFixture.pythonExecuter.CategorizeTransactions(importedTransactions, importedAiTransactions,
                userId.ToString());

            string?[] userCategories = importedTransactions.Select(it => it.UserCategory)
                .ToArray();

            string?[] expectedUserCategories = new string?[]
            {
                null,
                "Biedronka",
                null,
                "Auchan",
                "Żabka",
                null,
                "Praca"
            };

            userCategories.Should().BeEquivalentTo(expectedUserCategories,
                options => options.WithStrictOrdering());
        }
    }
}
