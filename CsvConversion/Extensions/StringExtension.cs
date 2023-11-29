using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvConversion.Extensions
{
    public static class StringExtension
    {
        public static string Beutify(this string text)
        {
            IEnumerable<string> splittedText = text.Split(" ")
                .Where(t => t.Equals("\n") || !string.IsNullOrWhiteSpace(t));

            return string.Join(" ", splittedText)
                .Trim();
        }

        public static string FirstCharToUpper(this string text)
        {
            return string.Concat(text[0].ToString().ToUpper(), text.Substring(1));
        }
    }
}
