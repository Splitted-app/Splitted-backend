using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.CsvModels
{
    public class TransactionCsv
    {
        public decimal Amount { get; set; }

        public string? Currency { get; set; }

        public DateTime Date { get; set; }

        public string? Description { get; set; }

        public string? Category { get; set; }

    }
}
