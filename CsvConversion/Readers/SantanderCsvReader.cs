using CsvConversion.Mappers;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Models.CsvModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CsvConversion.Readers
{
    public class SantanderCsvReader : BaseCsvReader
    {
        public SantanderCsvReader(IFormFile csvFile) : base(csvFile)
        {
        }


        private void SetCurrency(CsvReader csvReader) => SantanderMapper.currency = csvReader.GetField<string>(4)!;

        protected override CsvConfiguration SetConfiguration()
        {
            return new CsvConfiguration(cultureInfo: CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                Delimiter = ",",
                BadDataFound = null,
                HasHeaderRecord = false,
            };
        }

        protected override void SkipToHeaderRecord(CsvReader csvReader)
        {
            csvReader.Read();
            SetCurrency(csvReader);
        }

        protected override bool DetermineEndOfTransactions(CsvReader csvReader) => false;

        public override List<TransactionCsv> GetTransactions() => base.GetSpecificTransactions<SantanderMapper>(new[] { "dd-MM-yyyy", "yyyy-MM-dd" });
        
    }
}
