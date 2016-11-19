using Org.BouncyCastle.Math;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Xades_T_Validator.Helpers
{
    class CrlHelper
    {
        public static X509CrlEntry GetRevokedCertificateEntry(BigInteger serialNumber)
        {
            byte[] crlByteArray;

            using (WebClient webClient = new WebClient())
            {
                crlByteArray = webClient.DownloadData(new Uri("http://test.monex.sk/DTCCACrl/DTCCACrl.crl"));
            }

            X509CrlParser parser = new X509CrlParser();
            X509Crl crl = parser.ReadCrl(crlByteArray);

            return crl.GetRevokedCertificate(serialNumber);
        }
    }
}
