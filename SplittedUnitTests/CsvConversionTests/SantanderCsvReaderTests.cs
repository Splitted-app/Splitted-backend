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
    public class SantanderCsvReaderTests
    {
        [Fact]
        public void Test_GetTransactionsAndCheckAmountCurrencyDate()
        {
            string filePath = "../../../Data/FakeCsvData/SantanderCsv/SantanderTest1.csv";

            Mock<SantanderCsvReader> santanderCsvReaderMock = CsvReadersMock.GetMockedCsvReader<SantanderCsvReader>(filePath);
            SantanderCsvReader santanderCsvReader = santanderCsvReaderMock.Object;

            var expectedTransactionsData = new[]
            {
                new
                {
                    Amount = -89.35M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-21")
                },

                new
                {
                    Amount = -14.78M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-22")
                },

                new
                {
                    Amount = 657.12M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-28")
                },
            }
            .ToList();

            List<TransactionCsv>? importedTransactions = santanderCsvReader.GetTransactions();
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
        [InlineData("../../../Data/FakeCsvData/SantanderCsv/SantanderTest2.csv", 4)]
        [InlineData("../../../Data/FakeCsvData/SantanderCsv/SantanderTest3.csv", 11)]
        [InlineData("../../../Data/FakeCsvData/SantanderCsv/SantanderTest4.csv", 17)]
        public void Test_GetTransactionsAndCheckCount(string filePath, int count)
        {
            Mock<SantanderCsvReader> santanderCsvReaderMock = CsvReadersMock.GetMockedCsvReader<SantanderCsvReader>(filePath);
            SantanderCsvReader santanderCsvReader = santanderCsvReaderMock.Object;

            List<TransactionCsv>? importedTransactions = santanderCsvReader.GetTransactions();

            santanderCsvReaderMock.Protected()
                .Verify("SaveCsvFile", Times.Once());
            santanderCsvReaderMock.Protected()
                .Verify("DeleteCsvFile", Times.Once(), ItExpr.IsAny<string>());

            importedTransactions.Should().HaveCount(count);
        }

        [Fact]
        public void Test_GetTransactionsAndCheckTransactionType()
        {
            string filePath = "../../../Data/FakeCsvData/SantanderCsv/SantanderTest5.csv";

            Mock<SantanderCsvReader> santanderCsvReaderMock = CsvReadersMock.GetMockedCsvReader<SantanderCsvReader>(filePath);
            SantanderCsvReader santanderCsvReader = santanderCsvReaderMock.Object;

            List<TransactionTypeEnum> expectedTransactionTypes = new List<TransactionTypeEnum>
            {
                TransactionTypeEnum.Blik,
                TransactionTypeEnum.Card,
                TransactionTypeEnum.Transfer,
                TransactionTypeEnum.Blik,
                TransactionTypeEnum.Card,
                TransactionTypeEnum.Other,
            };

            List<TransactionCsv>? importedTransactions = santanderCsvReader.GetTransactions();
            List<TransactionTypeEnum> importedTransactionTypes = importedTransactions.Select(t => t.TransactionType)
                .ToList();

            importedTransactionTypes.Should().BeEquivalentTo(expectedTransactionTypes,
                options => options.WithStrictOrdering());
        }

        [Fact]
        public void Test_GetTransactionsAndCheckDescription()
        {
            string filePath = "../../../Data/FakeCsvData/SantanderCsv/SantanderTest6.csv";

            Mock<SantanderCsvReader> santanderCsvReaderMock = CsvReadersMock.GetMockedCsvReader<SantanderCsvReader>(filePath);
            SantanderCsvReader santanderCsvReader = santanderCsvReaderMock.Object;

            List<string> expectedDescriptions = new List<string>
            {
                "Allegro\nAllegro S.A.",
                "Stokrotka",
                "PRZELEW",
                "Przelew na telefon\nMateusz Tobiasz",
                "Biedronka",
                "Wypłata\nPłock"
            };

            List<TransactionCsv>? importedTransactions = santanderCsvReader.GetTransactions();
            List<string> importedDescriptions = importedTransactions.Select(t => t.Description)
                .ToList();

            importedDescriptions.Should().BeEquivalentTo(expectedDescriptions,
                options => options.WithStrictOrdering());
        }

        [Theory]
        [InlineData("../../../Data/FakeCsvData/SantanderCsv/SantanderBroken1.csv")]
        [InlineData("../../../Data/FakeCsvData/SantanderCsv/SantanderBroken2.csv")]
        [InlineData("../../../Data/FakeCsvData/SantanderCsv/SantanderBroken3.csv")]
        [InlineData("../../../Data/FakeCsvData/SantanderCsv/SantanderBroken4.tiff")]
        public void Test_GetTransactionsWithInvalidFile(string filePath)
        {
            Mock<SantanderCsvReader> santanderCsvReaderMock = CsvReadersMock.GetMockedCsvReader<SantanderCsvReader>(filePath);
            SantanderCsvReader santanderCsvReader = santanderCsvReaderMock.Object;

            List<TransactionCsv>? transactions = new List<TransactionCsv>();
            Action action = () => { transactions = santanderCsvReader.GetTransactions(); };

            action.Should().NotThrow();
            transactions.Should().BeNullOrEmpty();
        }

        [Theory]
        [InlineData("../../../Data/FakeCsvData/IngCsv/IngTest1.csv")]
        [InlineData("../../../Data/FakeCsvData/PkoCsv/PkoTest1.csv")]
        [InlineData("../../../Data/FakeCsvData/PekaoCsv/PekaoTest1.csv")]
        [InlineData("../../../Data/FakeCsvData/MbankCsv/MbankTest1.csv")]
        public void Test_GetTransactionsWithDifferentBankFile(string filePath)
        {
            Mock<SantanderCsvReader> santanderCsvReaderMock = CsvReadersMock.GetMockedCsvReader<SantanderCsvReader>(filePath);
            SantanderCsvReader santanderCsvReader = santanderCsvReaderMock.Object;

            List<TransactionCsv>? transactions = new List<TransactionCsv>();
            Action action = () => { transactions = santanderCsvReader.GetTransactions(); };

            action.Should().NotThrow();
            transactions.Should().BeNullOrEmpty();
        }
    }
}
