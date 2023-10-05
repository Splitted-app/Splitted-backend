using CsvHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvConversion.Extensions
{
    public static class CsvReaderExtension
    {
        public static bool TryGetRecord<T>(this CsvReader csvReader,  out T? convertedRecord)
        {
            try
            {
                convertedRecord = csvReader.GetRecord<T>();
                return true;
            }
            catch (Exception)
            {
                convertedRecord = default;
                return false;
            }
        }
    }
}
