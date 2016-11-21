using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xades_T_Validator.Attributes;
using Xades_T_Validator.Helpers;
using Xades_T_Validator.ValidationHandlers.Base;
using Xades_T_Validator.Wrappers;
using Xades_T_Validator.XMLHelpers;
using Xades_T_Validator.Extensions;
using System.Security.Cryptography.Xml;
using Xades_T_Validator.Enums;
using System.Collections;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography;

namespace Xades_T_Validator.ValidationHandlers
{
    [XadesTValidator(ExecutionOrder: 5, ValidationTaskName: "Overenie časovej pečiatky")]
    public class TimestampValidator : BaseXadesTValidator
    {
        public TimestampValidator(IEnumerable<XMLDocumentWrapper> documentWrappers) : base(documentWrappers)
        {
        }

        [XadesTValidationHandler(ExecutionOrder: 1, Description: "Overenie platnosti podpisového certifikátu časovej pečiatky voči času UtcNow a voči platnému poslednému CRL")]
        public ValidationError ValidationHandler1(XmlDocument xmlDoc, string xmlFileName)
        {
            ValidationError validationError = new ValidationError(xmlFileName, null);

            X509Certificate signerCert = GetSignerCertificate(xmlDoc);

            if (signerCert == null)
                return validationError.AppendErrorMessage("Timestamp signer certificate missing.");

            if (!signerCert.IsValidNow)
                return validationError.AppendErrorMessage("Timestamp signer certificate is not valid to current date.");

            X509CrlEntry crlEntry = CrlHelper.GetRevokedCertificateEntry(signerCert.SerialNumber);

            if (crlEntry != null)
                return validationError.AppendErrorMessage("Timestamp signer certificate is revoked.");

            return validationError;
        }

        [XadesTValidationHandler(ExecutionOrder: 2, Description: "Overenie MessageImprint z časovej pečiatky voči podpisu ds:SignatureValue")]
        public ValidationError ValidationHandler2(XmlDocument xmlDoc, string xmlFileName)
        {
            ValidationError validationError = new ValidationError(xmlFileName, null);

            TimeStampToken token = XmlNodeHelper.GetTimeStampToken(xmlDoc);

            byte[] timesStampDigestArray = token.TimeStampInfo.GetMessageImprintDigest();
            string hashAlgorithmId = token.TimeStampInfo.HashAlgorithm.Algorithm.Id;

            var signatureEle = xmlDoc.SelectXmlNode("//ds:Signature/ds:SignatureValue");

            if (signatureEle == null)
                return validationError.AppendErrorMessage("Missing SignatureValue element.");

            byte[] signatureValueByteArray = Convert.FromBase64String(signatureEle.InnerText);

            var signatureMethodAlgorithm = xmlDoc.SelectXmlNode("//ds:SignedInfo/ds:SignatureMethod").AtrValue("Algorithm");

            // TODO: Is this legit ?
            if (signatureMethodAlgorithm != "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256")
                return validationError.AppendErrorMessage($"Unknown SignatureMethod Algorithm {signatureMethodAlgorithm}.");

            System.Security.Cryptography.HashAlgorithm hashAlgo = System.Security.Cryptography.SHA256Managed.Create();

            var conputedSignatureByteArray = hashAlgo.ComputeHash(signatureValueByteArray);

            if (!StructuralComparisons.StructuralEqualityComparer.Equals(conputedSignatureByteArray, timesStampDigestArray))
            {
                return validationError.AppendErrorMessage("Missing SignatureValue element.");
            }

            return validationError;
        }

        #region Helpers
        private X509Certificate GetSignerCertificate(XmlDocument xmlDoc)
        {
            X509Certificate signerCertificate = null;

            try
            {
                TimeStampToken token = XmlNodeHelper.GetTimeStampToken(xmlDoc);
                var certificates = token.GetCertificates("Collection").GetMatches(null).Cast<X509Certificate>().ToList();

                foreach (X509Certificate certificate in certificates)
                {
                    string cerIssuerName = certificate.IssuerDN.ToString(true, new Dictionary<string, string>());
                    string signerIssuerName = token.SignerID.Issuer.ToString(true, new Dictionary<string, string>());

                    if (cerIssuerName == signerIssuerName && certificate.SerialNumber.Equals(token.SignerID.SerialNumber))
                    {
                        signerCertificate = certificate;
                        break;
                    }
                }
            }
            catch (Exception) { }

            return signerCertificate;
        }
        #endregion
    }
}
