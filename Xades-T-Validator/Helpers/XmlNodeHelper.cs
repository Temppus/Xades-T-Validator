using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xades_T_Validator.Extensions;
using Xades_T_Validator.Wrappers;

namespace Xades_T_Validator.XMLHelpers
{
    public static class XmlNodeHelper
    {
        public static X509Certificate GetX509Certificate(XMLDocumentWrapper docWrapper)
        {
            XmlDocument xmlDoc = docWrapper.XmlDoc;
            var encodedCertificate = xmlDoc.SelectXmlNode("//ds:Signature/ds:KeyInfo/ds:X509Data/ds:X509Certificate");

            if (encodedCertificate == null)
                return null;

            var parser = new X509CertificateParser();
            byte[] certificateBytes = Convert.FromBase64String(encodedCertificate.InnerText);
            var bouncyCertificate = parser.ReadCertificate(certificateBytes);

            return bouncyCertificate;
        }

        public static TimeStampToken GetTimeStampToken(XMLDocumentWrapper docWrapper)
        {
            XmlDocument xmlDoc = docWrapper.XmlDoc;
            var encapsulatedTimeStampEle = xmlDoc.SelectXmlNode("//xades:EncapsulatedTimeStamp");

            if (encapsulatedTimeStampEle == null)
                return null;

            var decodedTimeStamp = Convert.FromBase64String(encapsulatedTimeStampEle.InnerText);
            CmsSignedData cms = new CmsSignedData(decodedTimeStamp);
            TimeStampToken token = new TimeStampToken(cms);

            return token;
        }


    }
}
