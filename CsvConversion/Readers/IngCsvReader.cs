﻿using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Models.CsvModels;
using System.Globalization;
using CsvConversion.Mappers;
using Microsoft.AspNetCore.Http;

namespace CsvConversion.Readers
{
    public class IngCsvReader : BaseCsvReader
    {
        public IngCsvReader(IFormFile csvFile) : base(csvFile)
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
            while (csvReader.Read())
            {
                string? field = csvReader.GetField<string>(0);
                if (field is not null && field.Contains("Data"))
                {
                    csvReader.ReadHeader();
                    return;
                }
            }
        }

        protected override bool DetermineEndOfTransactions(CsvReader csvReader)
        {
            var field = csvReader.GetField<string>(0);
            if (field!.Equals("Dokument ma charakter informacyjny, nie stanowi dowodu księgowego")) return true;
            else return false;
        }

        public override List<TransactionCsv>? GetTransactions() => base.GetSpecificTransactions<IngMapper>(new[] { "dd-MM-yyyy", "yyyy-MM-dd" });

    }
}
