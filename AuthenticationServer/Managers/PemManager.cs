using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationServer.Managers
{
    public static class PemManager
    {
        public static RSACryptoServiceProvider LoadKey<T>(string key) where T : RsaKeyParameters
        {
            using (var reader = new StringReader(key))
            {
                PemReader pemReader = new PemReader(reader);
                T keyParameters = (T)pemReader.ReadObject();
                RSAParameters rsaParameters;

                if (keyParameters is RsaPrivateCrtKeyParameters)
                    rsaParameters = DotNetUtilities.ToRSAParameters(keyParameters as RsaPrivateCrtKeyParameters);
                else
                    rsaParameters = DotNetUtilities.ToRSAParameters(keyParameters);

                RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider();
                rsaCryptoServiceProvider.ImportParameters(rsaParameters);
                return rsaCryptoServiceProvider;
            }
        }
    }
}
