using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xades_T_Validator.Attributes;
using Xades_T_Validator.ValidationHandlers.Base;
using Xades_T_Validator.Wrappers;
using Xades_T_Validator.XMLHelpers;
//using System.Security.Cryptography.X509Certificates;

namespace Xades_T_Validator.ValidationHandlers
{
    [XadesTValidator(ExecutionOrder: 6, ValidationTaskName: "Overenie platnosti podpisového certifikátu")]
    public class CertificateValidator : BaseXadesTValidator
    {
        public CertificateValidator(IEnumerable<XMLDocumentWrapper> documentWrappers) : base(documentWrappers)
        {
        }

        [XadesTValidationHandler(ExecutionOrder: 1, Description: "Overenie platnosti podpisového certifikátu")]
        public ValidationError ValidationHandler1(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            TimeStampToken token = XmlNodeHelper.GetTimeStampToken(docWrapper);
            Org.BouncyCastle.X509.X509Certificate certificate = XmlNodeHelper.GetX509Certificate(docWrapper);

            if (certificate == null)
                return validationError.AppendErrorMessage("Nepodarilo sa nájsť certifikát");

            if (token == null)
                return validationError.AppendErrorMessage("Nepodarilo sa nájsť token");

            //Check certificate validity against timestamp token time
            try
            {
                certificate.CheckValidity(token.TimeStampInfo.GenTime);
            }
            catch(Exception ex)
            {
                return validationError.AppendErrorMessage("Platnosť podpisového certifikátu neodpovedá času z časovej pečiatky. ErrorMessage ->" + ex.Message);
            }
            
            //Check certificate validity against crl
            byte[] crlByteArray;

            using (WebClient webClient = new WebClient())
            {
                crlByteArray = webClient.DownloadData(new Uri("http://test.monex.sk/DTCCACrl/DTCCACrl.crl"));
            }

            X509CrlParser parser = new X509CrlParser();
            X509Crl crl = parser.ReadCrl(crlByteArray);
            X509CrlEntry entry = crl.GetRevokedCertificate(certificate.SerialNumber);

            if (entry == null)
                return validationError;

            if (entry.RevocationDate < token.TimeStampInfo.GenTime)
                return validationError.AppendErrorMessage("Platnosť certifikátu vypršala");
                
            return validationError;
        }
    }
}
