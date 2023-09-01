using CsvConversion.Mappers;
using CsvHelper;
using CsvHelper.Configuration;
using Models.CsvModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvConversion.Readers
{
    public class PekaoCsvReader : BaseCsvReader
    {
        public PekaoCsvReader(string path) : base(path)
        {
        }

        protected override CsvConfiguration SetConfiguration()
        {
            return new CsvConfiguration(cultureInfo: CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                Delimiter = ";",
                BadDataFound = null,
            };
        }

        protected override void SkipToHeaderRecord(CsvReader csvReader)
        {
            csvReader.Read();
            csvReader.ReadHeader();
        }

        public override List<TransactionCsv?> GetTransactions()
        {
            List<TransactionCsv?> transactions = new List<TransactionCsv?>();
            CsvConfiguration config = SetConfiguration();
            ConvertToUtf8(path);

            using (var reader = new StreamReader(path, Encoding.UTF8))
            using (var csvReader = new CsvReader(reader, config))
            {
                csvReader.Context.RegisterClassMap<PekaoMapper>();
                SetConverterOptions<DateTime>(csvReader, new[] { "dd.MM.yyyy", "yyyy.MM.dd" });
                SkipToHeaderRecord(csvReader);
                while (csvReader.Read())
                {
                    var transaction = csvReader.GetRecord<TransactionCsv?>();
                    transactions.Add(transaction);
                }
            }
            return transactions;
        }
    }
}
