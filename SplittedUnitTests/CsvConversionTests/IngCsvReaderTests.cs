using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using CsvConversion.Readers;
using FluentAssertions;
using MimeKit.Cryptography;
using Models.CsvModels;
using Models.Entities;
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
    public class IngCsvReaderTests
    {
        [Fact]
        public void Test_GetTransactionsAndCheckAmountCategoryCurrencyDate()
        {
            string filePath = "../../../Data/FakeCsvData/IngCsv/IngTest1.csv";

            Mock<IngCsvReader> ingCsvReaderMock = CsvReadersMock.GetMockedCsvReader<IngCsvReader>(filePath);
            IngCsvReader ingCsvReader = ingCsvReaderMock.Object;

            var expectedTransactionsData = new[]
            {
                new
                {
                    Amount = 254.5M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-21")
                },

                new
                {
                    Amount = -156.69M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-22")
                },

                new
                {
                    Amount = -55M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2024-01-01")
                },
            }
            .ToList();

            List<TransactionCsv>? importedTransactions = ingCsvReader.GetTransactions();
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
        [InlineData("../../../Data/FakeCsvData/IngCsv/IngTest2.csv", 7)]
        [InlineData("../../../Data/FakeCsvData/IngCsv/IngTest3.csv", 13)]
        [InlineData("../../../Data/FakeCsvData/IngCsv/IngTest4.csv", 21)]
        public void Test_GetTransactionsAndCheckCount(string filePath, int count)
        {
            Mock<IngCsvReader> ingCsvReaderMock = CsvReadersMock.GetMockedCsvReader<IngCsvReader>(filePath);
            IngCsvReader ingCsvReader = ingCsvReaderMock.Object;

            List<TransactionCsv>? importedTransactions = ingCsvReader.GetTransactions();

            ingCsvReaderMock.Protected()
                .Verify("SaveCsvFile", Times.Once());
            ingCsvReaderMock.Protected()
                .Verify("DeleteCsvFile", Times.Once(), ItExpr.IsAny<string>());

            importedTransactions.Should().HaveCount(count);
        }

        [Fact]
        public void Test_GetTransactionsAndCheckTransactionType()
        {
            string filePath = "../../../Data/FakeCsvData/IngCsv/IngTest5.csv";

            Mock<IngCsvReader> ingCsvReaderMock = CsvReadersMock.GetMockedCsvReader<IngCsvReader>(filePath);
            IngCsvReader ingCsvReader = ingCsvReaderMock.Object;

            List<TransactionTypeEnum> expectedTransactionTypes = new List<TransactionTypeEnum>
            {
                TransactionTypeEnum.Transfer,
                TransactionTypeEnum.Card,
                TransactionTypeEnum.Blik,
                TransactionTypeEnum.Transfer,
                TransactionTypeEnum.Blik,
                TransactionTypeEnum.Other,
            };

            List<TransactionCsv>? importedTransactions = ingCsvReader.GetTransactions();
            List<TransactionTypeEnum> importedTransactionTypes = importedTransactions.Select(t => t.TransactionType)
                .ToList();

            importedTransactionTypes.Should().BeEquivalentTo(expectedTransactionTypes, 
                options => options.WithStrictOrdering());
        }

        [Fact]
        public void Test_GetTransactionsAndCheckDescription()
        {
            string filePath = "../../../Data/FakeCsvData/IngCsv/IngTest6.csv";

            Mock<IngCsvReader> ingCsvReaderMock = CsvReadersMock.GetMockedCsvReader<IngCsvReader>(filePath);
            IngCsvReader ingCsvReader = ingCsvReaderMock.Object;

            List<string> expectedDescriptions = new List<string>
            {
                "Mateusz Tobiasz \n Nowy przelew 1",
                "Stokrotka",
                "Allegro",
                "Abc abc \n Przelew własny",
                "Abc abc \n Od a Do b",
                "Fake info \n Wypłata"
            };

            List<TransactionCsv>? importedTransactions = ingCsvReader.GetTransactions();
            List<string> importedDescriptions = importedTransactions.Select(t => t.Description)
                .ToList();

            importedDescriptions.Should().BeEquivalentTo(expectedDescriptions, 
                options => options.WithStrictOrdering());
        }

        [Theory]
        [InlineData("../../../Data/FakeCsvData/IngCsv/IngBroken1.csv")]
        [InlineData("../../../Data/FakeCsvData/IngCsv/IngBroken2.csv")]
        [InlineData("../../../Data/FakeCsvData/IngCsv/IngBroken3.csv")]
        [InlineData("../../../Data/FakeCsvData/IngCsv/IngBroken4.jpg")]
        public void Test_GetTransactionsWithInvalidFile(string filePath)
        {
            Mock<IngCsvReader> ingCsvReaderMock = CsvReadersMock.GetMockedCsvReader<IngCsvReader>(filePath);
            IngCsvReader ingCsvReader = ingCsvReaderMock.Object;

            List<TransactionCsv>? transactions = new List<TransactionCsv>();
            Action action = () => { transactions = ingCsvReader.GetTransactions(); };

            action.Should().NotThrow();
            transactions.Should().BeNullOrEmpty();
        }

        [Theory]
        [InlineData("../../../Data/FakeCsvData/MbankCsv/MbankTest1.csv")]
        [InlineData("../../../Data/FakeCsvData/PkoCsv/PkoTest1.csv")]
        [InlineData("../../../Data/FakeCsvData/PekaoCsv/PekaoTest1.csv")]
        [InlineData("../../../Data/FakeCsvData/SantanderCsv/SantanderTest1.csv")]
        public void Test_GetTransactionsWithDifferentBankFile(string filePath)
        {
            Mock<IngCsvReader> ingCsvReaderMock = CsvReadersMock.GetMockedCsvReader<IngCsvReader>(filePath);
            IngCsvReader ingCsvReader = ingCsvReaderMock.Object;

            List<TransactionCsv>? transactions = new List<TransactionCsv>();
            Action action = () => { transactions = ingCsvReader.GetTransactions(); };

            action.Should().NotThrow();
            transactions.Should().BeNullOrEmpty();
        }
    }
}
