using CsvConversion.Mappers;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
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
        public PekaoCsvReader(IFormFile csvFile) : base(csvFile)
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

        protected override bool DetermineEndOfTransactions(CsvReader csvReader)
        {
            var field = csvReader.GetField<string>(0);
            if (field!.Equals("")) return true;
            else return false;
        }

        public override List<TransactionCsv>? GetTransactions() => base.GetSpecificTransactions<PekaoMapper>(
            new[] { "dd-MM-yyyy", "yyyy-MM-dd", "dd.MM.yyyy", "yyyy.MM.dd" });
       
    }
}
