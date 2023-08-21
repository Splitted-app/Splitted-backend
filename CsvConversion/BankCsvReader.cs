using CsvHelper;
using CsvHelper.Configuration;
using Splitted_backend.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvConversion
{
    public abstract class BankCsvReader
    {
        protected string path;

        public BankCsvReader(string path)
        {
            this.path = path;   
        }

        protected abstract CsvConfiguration SetConfiguration();

        protected abstract void SkipToHeaderRecord(CsvReader csvReader);

        public abstract List<TransactionCsv?> GetTransactions();
    }
}
