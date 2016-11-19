using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xades_T_Validator.Attributes;
using Xades_T_Validator.ValidationHandlers.Base;
using Xades_T_Validator.Wrappers;
using Xades_T_Validator.Extensions;
using Xades_T_Validator.Enums;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto.Tls;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace Xades_T_Validator.ValidationHandlers
{
    [XadesTValidator(ExecutionOrder: 3, ValidationTaskName: "Core validácia podľa špecifikácie XML Signature")]
    public class CoreValidator : BaseXadesTValidator
    {
        public CoreValidator(IEnumerable<XMLDocumentWrapper> documentWrappers) : base(documentWrappers)
        {
        }

        [XadesTValidationHandler(ExecutionOrder: 1, Description: "Overenie hodnoty podpisu ds:SignatureValue a referencií v ds:SignedInfo.")]
        public ValidationError ValidationHandler1(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            var referencesNodes = xmlDoc.DocumentElement.SelectNodes("//ds:Signature/ds:SignedInfo/ds:Reference[@Type='http://www.w3.org/2000/09/xmldsig#Manifest']", xmlDoc.NameSpaceManager());

            foreach (XmlNode refNode in referencesNodes)
            {
                string uriTarget = refNode.Attributes["URI"]?.Value?.Substring(1);

                var manifestNode = xmlDoc.DocumentElement.SelectSingleNode($"//ds:Manifest[@Id='{uriTarget}']", xmlDoc.NameSpaceManager());

                if (manifestNode == null)
                    validationError.AppendErrorMessage($"Couldnt find Manifest element with id : {uriTarget}.");

                // Reference digest method
                string digestAlgorithm = refNode.SelectSingleNode("//ds:DigestMethod", xmlDoc.NameSpaceManager())?.Attributes["Algorithm"]?.Value;

                if (!ValidationEnums.HashAlgorithms.SHAMappings.ContainsKey(digestAlgorithm))
                    validationError.AppendErrorMessage($"Invalid digest method algorithm : {digestAlgorithm}");

                var transformNodes = refNode.SelectNodes("ds:Transforms/ds:Transform", xmlDoc.NameSpaceManager());

                // Should be only one algorithm (Canonicalization - omit comments)
                foreach (XmlNode transformNode in transformNodes)
                {
                    string transformAlgorithm = transformNode.Attributes["Algorithm"]?.Value;

                    if (transformAlgorithm != ValidationEnums.Canonicalization.CanonicalizationMethod)
                    {
                        validationError.AppendErrorMessage($"Invalid transform algorithm : {transformAlgorithm}");
                        break;
                    }

                    XmlDocument manifestDoc = new XmlDocument();
                    xmlDoc.PreserveWhitespace = true;
                    manifestDoc.LoadXml(manifestNode.OuterXml);

                    XmlDsigC14NTransform c14n = new XmlDsigC14NTransform(false);
                    c14n.LoadInput(manifestDoc);
                    var outputArray = c14n.GetDigestedOutput(ValidationEnums.HashAlgorithms.SHAMappings[digestAlgorithm]);
                    string digestOutputBase64String = Convert.ToBase64String(outputArray);

                    // Retrieve expected digest
                    string xmlDigestValueBase64String = refNode.SelectSingleNode("ds:DigestValue", xmlDoc.NameSpaceManager())?.InnerText;

                    if (digestOutputBase64String != xmlDigestValueBase64String)
                    {
                        validationError.AppendErrorMessage("Digest values do not match.");
                    }
                }
            }

            return validationError;
        }
    }
}
