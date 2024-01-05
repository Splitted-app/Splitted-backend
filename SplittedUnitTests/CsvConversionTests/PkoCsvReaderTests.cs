using CsvConversion.Readers;
using FluentAssertions;
using Models.CsvModels;
using Models.Enums;
using Moq;
using Moq.Protected;
using SplittedUnitTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.CsvConversionTests
{
    public class PkoCsvReaderTests
    {
        [Fact]
        public void Test_GetTransactionsAndCheckAmountCurrencyDate()
        {
            string filePath = "../../../Data/FakeCsvData/PkoCsv/PkoTest1.csv";

            Mock<PkoCsvReader> pkoCsvReaderMock = CsvReadersMock.GetMockedCsvReader<PkoCsvReader>(filePath);
            PkoCsvReader pkoCsvReader = pkoCsvReaderMock.Object;

            var expectedTransactionsData = new[]
            {
                new
                {
                    Amount = -59.89M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-21")
                },

                new
                {
                    Amount = 2000M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-27")
                },

                new
                {
                    Amount = -129.47M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-29")
                },
            }
            .ToList();

            List<TransactionCsv>? importedTransactions = pkoCsvReader.GetTransactions();
            var importedTransactionsData = importedTransactions.Select(t => new
            {
                Amount = t.Amount,
                Currency = t.Currency,
                Date = t.Date
            })
            .ToList();

            importedTransactionsData.Should().BeEquivalentTo(expectedTransactionsData,
                options => options.WithStrictOrdering());
        }

        [Theory]
        [InlineData("../../../Data/FakeCsvData/PkoCsv/PkoTest2.csv", 5)]
        [InlineData("../../../Data/FakeCsvData/PkoCsv/PkoTest3.csv", 10)]
        [InlineData("../../../Data/FakeCsvData/PkoCsv/PkoTest4.csv", 15)]
        public void Test_GetTransactionsAndCheckCount(string filePath, int count)
        {
            Mock<PkoCsvReader> pkoCsvReaderMock = CsvReadersMock.GetMockedCsvReader<PkoCsvReader>(filePath);
            PkoCsvReader pkoCsvReader = pkoCsvReaderMock.Object;

            List<TransactionCsv>? importedTransactions = pkoCsvReader.GetTransactions();

            pkoCsvReaderMock.Protected()
                .Verify("SaveCsvFile", Times.Once());
            pkoCsvReaderMock.Protected()
                .Verify("DeleteCsvFile", Times.Once(), ItExpr.IsAny<string>());

            importedTransactions.Should().HaveCount(count);
        }

        [Fact]
        public void Test_GetTransactionsAndCheckTransactionType()
        {
            string filePath = "../../../Data/FakeCsvData/PkoCsv/PkoTest5.csv";

            Mock<PkoCsvReader> pkoCsvReaderMock = CsvReadersMock.GetMockedCsvReader<PkoCsvReader>(filePath);
            PkoCsvReader pkoCsvReader = pkoCsvReaderMock.Object;

            List<TransactionTypeEnum> expectedTransactionTypes = new List<TransactionTypeEnum>
            {
                TransactionTypeEnum.Card,
                TransactionTypeEnum.Transfer,
                TransactionTypeEnum.Transfer,
                TransactionTypeEnum.Other,
                TransactionTypeEnum.Blik,
                TransactionTypeEnum.Blik,
            };

            List<TransactionCsv>? importedTransactions = pkoCsvReader.GetTransactions();
            List<TransactionTypeEnum> importedTransactionTypes = importedTransactions.Select(t => t.TransactionType)
                .ToList();

            importedTransactionTypes.Should().BeEquivalentTo(expectedTransactionTypes,
                options => options.WithStrictOrdering());
        }

        [Fact]
        public void Test_GetTransactionsAndCheckDescription()
        {
            string filePath = "../../../Data/FakeCsvData/PkoCsv/PkoTest6.csv";

            Mock<PkoCsvReader> pkoCsvReaderMock = CsvReadersMock.GetMockedCsvReader<PkoCsvReader>(filePath);
            PkoCsvReader pkoCsvReader = pkoCsvReaderMock.Object;

            List<string> expectedDescriptions = new List<string>
            {
                "BIEDRONKA Miasto PLOCK Kraj POLSKA",
                "Mateusz Tobiasz fakeAddress\n Przelew",
                "fakeName fakeAddress\n fakeTitle",
                "Bankomat PKO",
                "name PRZELEW NA TELEFONOD fakeNumber DO fakeNumber",
                "allegro.pl"
            };

            List<TransactionCsv>? importedTransactions = pkoCsvReader.GetTransactions();
            List<string> importedDescriptions = importedTransactions.Select(t => t.Description)
                .ToList();

            importedDescriptions.Should().BeEquivalentTo(expectedDescriptions,
                options => options.WithStrictOrdering());
        }

        [Theory]
        [InlineData("../../../Data/FakeCsvData/PkoCsv/PkoBroken1.csv")]
        [InlineData("../../../Data/FakeCsvData/PkoCsv/PkoBroken2.csv")]
        [InlineData("../../../Data/FakeCsvData/PkoCsv/PkoBroken3.csv")]
        [InlineData("../../../Data/FakeCsvData/PkoCsv/PkoBroken4.pdf")]
        public void Test_GetTransactionsWithInvalidFile(string filePath)
        {
            Mock<PkoCsvReader> pkoCsvReaderMock = CsvReadersMock.GetMockedCsvReader<PkoCsvReader>(filePath);
            PkoCsvReader pkoCsvReader = pkoCsvReaderMock.Object;

            List<TransactionCsv>? transactions = new List<TransactionCsv>();
            Action action = () => { transactions = pkoCsvReader.GetTransactions(); };

            action.Should().NotThrow();
            transactions.Should().BeNullOrEmpty();
        }

        [Theory]
        [InlineData("../../../Data/FakeCsvData/MbankCsv/MbankTest1.csv")]
        [InlineData("../../../Data/FakeCsvData/IngCsv/IngTest1.csv")]
        [InlineData("../../../Data/FakeCsvData/PekaoCsv/PekaoTest1.csv")]
        [InlineData("../../../Data/FakeCsvData/SantanderCsv/SantanderTest1.csv")]
        public void Test_GetTransactionsWithDifferentBankFile(string filePath)
        {
            Mock<PkoCsvReader> pkoCsvReaderMock = CsvReadersMock.GetMockedCsvReader<PkoCsvReader>(filePath);
            PkoCsvReader pkoCsvReader = pkoCsvReaderMock.Object;

            List<TransactionCsv>? transactions = new List<TransactionCsv>();
            Action action = () => { transactions = pkoCsvReader.GetTransactions(); };

            action.Should().NotThrow();
            transactions.Should().BeNullOrEmpty();
        }
    }
}
