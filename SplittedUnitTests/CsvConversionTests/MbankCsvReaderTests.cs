using CsvConversion.Readers;
using FluentAssertions;
using Models.CsvModels;
using Models.Enums;
using Moq.Protected;
using Moq;
using SplittedUnitTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.CsvConversionTests
{
    public class MbankCsvReaderTests
    {
        [Fact]
        public void Test_GetTransactionsAndCheckAmountCategoryCurrencyDate()
        {
            string filePath = "../../../Data/FakeCsvData/MbankCsv/MbankTest1.csv";

            Mock<MbankCsvReader> mbankCsvReaderMock = CsvReadersMock.GetMockedCsvReader<MbankCsvReader>(filePath);
            MbankCsvReader mbankCsvReader = mbankCsvReaderMock.Object;

            var expectedTransactionsData = new[]
            {
                new
                {
                    Amount = -25.78M,
                    BankCategory = "Chemia domowa",
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-21"),
                },

                new
                {
                    Amount = 100M,
                    BankCategory = "Bez kategorii",
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-22")
                },

                new
                {
                    Amount = -98.56M,
                    BankCategory = "Żywność",
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-27")
                },
            }
            .ToList();

            List<TransactionCsv>? importedTransactions = mbankCsvReader.GetTransactions();
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
        [InlineData("../../../Data/FakeCsvData/MbankCsv/MbankTest2.csv", 6)]
        [InlineData("../../../Data/FakeCsvData/MbankCsv/MbankTest3.csv", 15)]
        [InlineData("../../../Data/FakeCsvData/MbankCsv/MbankTest4.csv", 21)]
        public void Test_GetTransactionsAndCheckCount(string filePath, int count)
        {
            Mock<MbankCsvReader> mbankCsvReaderMock = CsvReadersMock.GetMockedCsvReader<MbankCsvReader>(filePath);
            MbankCsvReader mbankCsvReader = mbankCsvReaderMock.Object;

            List<TransactionCsv>? importedTransactions = mbankCsvReader.GetTransactions();

            mbankCsvReaderMock.Protected()
                .Verify("SaveCsvFile", Times.Once());
            mbankCsvReaderMock.Protected()
                .Verify("DeleteCsvFile", Times.Once(), ItExpr.IsAny<string>());

            importedTransactions.Should().HaveCount(count);
        }

        [Fact]
        public void Test_GetTransactionsAndCheckTransactionType()
        {
            string filePath = "../../../Data/FakeCsvData/MbankCsv/MbankTest5.csv";

            Mock<MbankCsvReader> mbankCsvReaderMock = CsvReadersMock.GetMockedCsvReader<MbankCsvReader>(filePath);
            MbankCsvReader mbankCsvReader = mbankCsvReaderMock.Object;

            List<TransactionTypeEnum> expectedTransactionTypes = new List<TransactionTypeEnum>
            {
                TransactionTypeEnum.Card,
                TransactionTypeEnum.Blik,
                TransactionTypeEnum.Other,
                TransactionTypeEnum.Card,
                TransactionTypeEnum.Blik,
                TransactionTypeEnum.Transfer,
            };

            List<TransactionCsv>? importedTransactions = mbankCsvReader.GetTransactions();
            List<TransactionTypeEnum> importedTransactionTypes = importedTransactions.Select(t => t.TransactionType)
                .ToList();

            importedTransactionTypes.Should().BeEquivalentTo(expectedTransactionTypes,
                options => options.WithStrictOrdering());
        }

        [Fact]
        public void Test_GetTransactionsAndCheckDescription()
        {
            string filePath = "../../../Data/FakeCsvData/MbankCsv/MbankTest6.csv";

            Mock<MbankCsvReader> mbankCsvReaderMock = CsvReadersMock.GetMockedCsvReader<MbankCsvReader>(filePath);
            MbankCsvReader mbankCsvReader = mbankCsvReaderMock.Object;

            List<string> expectedDescriptions = new List<string>
            {
                "Rossman",
                "BLIK P2P-PRZYCHODZĄCY Mateusz Tobiasz",
                "Wypłata z bankomatu",
                "Carrefour",
                "BLIK P2P-WYCHODZĄCY Allegro",
                "Przelew"
            };

            List<TransactionCsv>? importedTransactions = mbankCsvReader.GetTransactions();
            List<string> importedDescriptions = importedTransactions.Select(t => t.Description)
                .ToList();

            importedDescriptions.Should().BeEquivalentTo(expectedDescriptions,
                options => options.WithStrictOrdering());
        }

        [Theory]
        [InlineData("../../../Data/FakeCsvData/MbankCsv/MbankBroken1.csv")]
        [InlineData("../../../Data/FakeCsvData/MbankCsv/MbankBroken2.csv")]
        [InlineData("../../../Data/FakeCsvData/MbankCsv/MbankBroken3.csv")]
        [InlineData("../../../Data/FakeCsvData/MbankCsv/MbankBroken4.gif")]
        public void Test_GetTransactionsWithInvalidFile(string filePath)
        {
            Mock<MbankCsvReader> mbankCsvReaderMock = CsvReadersMock.GetMockedCsvReader<MbankCsvReader>(filePath);
            MbankCsvReader mbankCsvReader = mbankCsvReaderMock.Object;

            List<TransactionCsv>? transactions = new List<TransactionCsv>();
            Action action = () => { transactions = mbankCsvReader.GetTransactions(); };

            action.Should().NotThrow();
            transactions.Should().BeNullOrEmpty();
        }

        [Theory]
        [InlineData("../../../Data/FakeCsvData/IngCsv/IngTest1.csv")]
        [InlineData("../../../Data/FakeCsvData/PkoCsv/PkoTest1.csv")]
        [InlineData("../../../Data/FakeCsvData/PekaoCsv/PekaoTest1.csv")]
        [InlineData("../../../Data/FakeCsvData/SantanderCsv/SantanderTest1.csv")]
        public void Test_GetTransactionsWithDifferentBankFile(string filePath)
        {
            Mock<MbankCsvReader> mbankCsvReaderMock = CsvReadersMock.GetMockedCsvReader<MbankCsvReader>(filePath);
            MbankCsvReader mbankCsvReader = mbankCsvReaderMock.Object;

            List<TransactionCsv>? transactions = new List<TransactionCsv>();
            Action action = () => { transactions = mbankCsvReader.GetTransactions(); };

            action.Should().NotThrow();
            transactions.Should().BeNullOrEmpty();
        }
    }
}
