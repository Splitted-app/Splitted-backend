using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalServices.Extensions
{
    public static class StringExtension
    {
        public static string GetExtensionFromBase64String(this string validBase64String)
        {
            string[] semicolonSplitted = validBase64String.Split(';');
            string[] slashSplitted = semicolonSplitted[0].Split('/');

            return "." + slashSplitted[1];
        }
    }
}
