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
    public class PekaoCsvReaderTests
    {
        [Fact]
        public void Test_GetTransactionsAndCheckAmountCategoryCurrencyDate()
        {
            string filePath = "../../../Data/FakeCsvData/PekaoCsv/PekaoTest1.csv";

            Mock<PekaoCsvReader> pekaoCsvReaderMock = CsvReadersMock.GetMockedCsvReader<PekaoCsvReader>(filePath);
            PekaoCsvReader pekaoCsvReader = pekaoCsvReaderMock.Object;

            var expectedTransactionsData = new[]
            {
                new
                {
                    Amount = -73.29M,
                    BankCategory = "Chemia domowa",
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-21")
                },

                new
                {
                    Amount = 56.32M,
                    BankCategory = "Bez kategorii",
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-23")
                },

                new
                {
                    Amount = -289.13M,
                    BankCategory = "Żywność",
                    Currency = "PLN",
                    Date = DateTime.Parse("2024-01-02")
                },
            }
            .ToList();

            List<TransactionCsv>? importedTransactions = pekaoCsvReader.GetTransactions();
            var importedTransactionsData = importedTransactions.Select(t => new
            {
                Amount = t.Amount,
                BankCategory = t.BankCategory,
                Currency = t.Currency,
                Date = t.Date
            })
            .ToList();

            importedTransactionsData.Should().BeEquivalentTo(expectedTransactionsData,
                options => options.WithStrictOrdering());
        }

        [Theory]
        [InlineData("../../../Data/FakeCsvData/PekaoCsv/PekaoTest2.csv", 5)]
        [InlineData("../../../Data/FakeCsvData/PekaoCsv/PekaoTest3.csv", 12)]
        [InlineData("../../../Data/FakeCsvData/PekaoCsv/PekaoTest4.csv", 19)]
        public void Test_GetTransactionsAndCheckCount(string filePath, int count)
        {
            Mock<PekaoCsvReader> pekaoCsvReaderMock = CsvReadersMock.GetMockedCsvReader<PekaoCsvReader>(filePath);
            PekaoCsvReader pekaoCsvReader = pekaoCsvReaderMock.Object;

            List<TransactionCsv>? importedTransactions = pekaoCsvReader.GetTransactions();

            pekaoCsvReaderMock.Protected()
                .Verify("SaveCsvFile", Times.Once());
            pekaoCsvReaderMock.Protected()
                .Verify("DeleteCsvFile", Times.Once(), ItExpr.IsAny<string>());

            importedTransactions.Should().HaveCount(count);
        }

        [Fact]
        public void Test_GetTransactionsAndCheckTransactionType()
        {
            string filePath = "../../../Data/FakeCsvData/PekaoCsv/PekaoTest5.csv";

            Mock<PekaoCsvReader> pekaoCsvReaderMock = CsvReadersMock.GetMockedCsvReader<PekaoCsvReader>(filePath);
            PekaoCsvReader pekaoCsvReader = pekaoCsvReaderMock.Object;

            List<TransactionTypeEnum> expectedTransactionTypes = new List<TransactionTypeEnum>
            {
                TransactionTypeEnum.Card,
                TransactionTypeEnum.Blik,
                TransactionTypeEnum.Other,
                TransactionTypeEnum.Card,
                TransactionTypeEnum.Transfer,
                TransactionTypeEnum.Blik,
            };

            List<TransactionCsv>? importedTransactions = pekaoCsvReader.GetTransactions();
            List<TransactionTypeEnum> importedTransactionTypes = importedTransactions.Select(t => t.TransactionType)
                .ToList();

            importedTransactionTypes.Should().BeEquivalentTo(expectedTransactionTypes,
                options => options.WithStrictOrdering());
        }

        [Fact]
        public void Test_GetTransactionsAndCheckDescription()
        {
            string filePath = "../../../Data/FakeCsvData/PekaoCsv/PekaoTest6.csv";

            Mock<PekaoCsvReader> pekaoCsvReaderMock = CsvReadersMock.GetMockedCsvReader<PekaoCsvReader>(filePath);
            PekaoCsvReader pekaoCsvReader = pekaoCsvReaderMock.Object;

            List<string> expectedDescriptions = new List<string>
            {
                "BIEDRONKA",
                "Mateusz TObiasz \nPrzelew na telefonOd: fakeNumber Do: fakeNumber",
                "Wypłata",
                "ZABKA",
                "Przelew",
                "Mateusz TObiasz Płock\nPrzelew na telefonOd: fakeNumber Do: fakeNumber",
            };

            List<TransactionCsv>? importedTransactions = pekaoCsvReader.GetTransactions();
            List<string> importedDescriptions = importedTransactions.Select(t => t.Description)
                .ToList();

            importedDescriptions.Should().BeEquivalentTo(expectedDescriptions,
                options => options.WithStrictOrdering());
        }

        [Theory]
        [InlineData("../../../Data/FakeCsvData/PekaoCsv/PekaoBroken1.csv")]
        [InlineData("../../../Data/FakeCsvData/PekaoCsv/PekaoBroken2.csv")]
        [InlineData("../../../Data/FakeCsvData/PekaoCsv/PekaoBroken3.csv")]
        [InlineData("../../../Data/FakeCsvData/PekaoCsv/PekaoBroken4.png")]
        public void Test_GetTransactionsWithInvalidFile(string filePath)
        {
            Mock<PekaoCsvReader> pekaoCsvReaderMock = CsvReadersMock.GetMockedCsvReader<PekaoCsvReader>(filePath);
            PekaoCsvReader pekaoCsvReader = pekaoCsvReaderMock.Object;

            List<TransactionCsv>? transactions = new List<TransactionCsv>();
            Action action = () => { transactions = pekaoCsvReader.GetTransactions(); };

            action.Should().NotThrow();
            transactions.Should().BeNullOrEmpty();
        }

        [Theory]
        [InlineData("../../../Data/FakeCsvData/IngCsv/IngTest1.csv")]
        [InlineData("../../../Data/FakeCsvData/PkoCsv/PkoTest1.csv")]
        [InlineData("../../../Data/FakeCsvData/MbankCsv/MbankTest1.csv")]
        [InlineData("../../../Data/FakeCsvData/SantanderCsv/SantanderTest1.csv")]
        public void Test_GetTransactionsWithDifferentBankFile(string filePath)
        {
            Mock<PekaoCsvReader> pekaoCsvReaderMock = CsvReadersMock.GetMockedCsvReader<PekaoCsvReader>(filePath);
            PekaoCsvReader pekaoCsvReader = pekaoCsvReaderMock.Object;

            List<TransactionCsv>? transactions = new List<TransactionCsv>();
            Action action = () => { transactions = pekaoCsvReader.GetTransactions(); };

            action.Should().NotThrow();
            transactions.Should().BeNullOrEmpty();
        }
    }
}
