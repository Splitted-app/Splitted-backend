using CsvConversion.Extensions;
using CsvConversion.Mappers;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.AspNetCore.Http;
using Models.CsvModels;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace CsvConversion.Readers
{
    public abstract class BaseCsvReader
    {
        protected IFormFile csvFile;


        public BaseCsvReader(IFormFile csvFile)
        {
            this.csvFile = csvFile;
        }


        private string SaveCsvFile()
        {
            string fileName = Guid.NewGuid().ToString() + "_" + csvFile.FileName;

            using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
            {
                csvFile.CopyTo(fileStream);
            }

            ConvertToUtf8(fileName);
            return fileName;
        }

        private void ConvertToUtf8(string fileName)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            EncodingConverter.Encode(fileName, Encoding.GetEncoding(1250), Encoding.UTF8);
        }

        private void SetConverterOptions<T>(CsvReader csvReader, string[] formats)
        {
            var options = new TypeConverterOptions { Formats = formats };
            csvReader.Context.TypeConverterOptionsCache.AddOptions<T>(options);
        }

        private bool TrySkipToHeaderRecord(CsvReader csvReader)
        {
            try
            {
                SkipToHeaderRecord(csvReader);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected List<TransactionCsv>? GetSpecificTransactions<T>(string[] formats) where T : ClassMap
        {
            List<TransactionCsv>? transactions = new List<TransactionCsv>();
            CsvConfiguration config = SetConfiguration();
            string fileName = SaveCsvFile();

            using (var reader = new StreamReader(fileName, Encoding.UTF8))
            using (var csvReader = new CsvReader(reader, config))
            {
                csvReader.Context.RegisterClassMap<T>();
                SetConverterOptions<DateTime>(csvReader, formats);

                if (!TrySkipToHeaderRecord(csvReader))
                    return transactions;

                while (csvReader.Read() && !DetermineEndOfTransactions(csvReader))
                {
                    TransactionCsv? transactionCsv;
                    bool ifConverted = csvReader.TryGetRecord(out transactionCsv);

                    if (ifConverted)
                        transactions.Add(transactionCsv!);
                    else
                    {
                        transactions = null;
                        break;
                    }
                }
            }

            File.Delete(fileName);
            return transactions;
        }

        protected abstract bool DetermineEndOfTransactions(CsvReader csvReader);

        protected abstract CsvConfiguration SetConfiguration();

        protected abstract void SkipToHeaderRecord(CsvReader csvReader);

        public abstract List<TransactionCsv>? GetTransactions();
    }
}
