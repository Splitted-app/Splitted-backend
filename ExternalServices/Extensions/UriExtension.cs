using Models.EmailModels;
using System.Collections.Specialized;
using System.Web;

namespace ExternalServices.Extensions
{
    public static class UriExtension
    {
        public static Uri AddParameter(this Uri uri, string parameterName, string parameterValue)
        {
            UriBuilder uriBuilder = new UriBuilder(uri);
            NameValueCollection query = HttpUtility.ParseQueryString(uriBuilder.Query, System.Text.Encoding.UTF8);
            query[parameterName] = parameterValue;
            uriBuilder.Query = query.ToString();

            return uriBuilder.Uri;
        }
    }
}
