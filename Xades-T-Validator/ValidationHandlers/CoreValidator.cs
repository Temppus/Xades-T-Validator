using System;
using System.Collections.Generic;
using System.Xml;
using Xades_T_Validator.Attributes;
using Xades_T_Validator.ValidationHandlers.Base;
using Xades_T_Validator.Wrappers;
using Xades_T_Validator.Extensions;
using Xades_T_Validator.Enums;
using System.Security.Cryptography.Xml;
using System.IO;
using Xades_T_Validator.XMLHelpers;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Xades_T_Validator.Helpers;

namespace Xades_T_Validator.ValidationHandlers
{
    [XadesTValidator(ExecutionOrder: 3, ValidationTaskName: "Core validácia podľa špecifikácie XML Signature")]
    public class CoreValidator : BaseXadesTValidator
    {
        public CoreValidator(IEnumerable<XMLDocumentWrapper> documentWrappers) : base(documentWrappers)
        {
        }

        [XadesTValidationHandler(ExecutionOrder: 1, Description: "Overenie hodnôt odtlačkov ds:DigestValue")]
        public ValidationError DigestValueVerificationHandler(XmlDocument xmlDoc, string xmlFileName)
        {
            ValidationError validationError = new ValidationError(xmlFileName, null);

            if (!ValidationEnums.ReferenceTypeConstraints.Mappings.ContainsKey("ds:Manifest"))
                return validationError.AppendErrorMessage("Signature reference type was not found.");

            var referenceType = ValidationEnums.ReferenceTypeConstraints.Mappings["ds:Manifest"];

            var referencesNodes = xmlDoc.SelectXmlNodes($"//ds:Signature/ds:SignedInfo/ds:Reference[@Type='{referenceType}']");

            foreach (XmlNode refNode in referencesNodes)
            {
                string uriTarget = refNode.AtrValue("URI")?.Substring(1);

                var manifestNode = xmlDoc.SelectXmlNode($"//ds:Manifest[@Id='{uriTarget}']");

                if (manifestNode == null)
                    return validationError.AppendErrorMessage($"Couldnt find Manifest element with id : {uriTarget}.");

                // Reference digest method
                string digestAlgorithm = refNode.SelectXmlNode("//ds:DigestMethod")?.AtrValue("Algorithm");

                if (!ValidationEnums.HashAlgorithms.SHAMappings.ContainsKey(digestAlgorithm))
                    return validationError.AppendErrorMessage($"Invalid digest method algorithm : {digestAlgorithm}");

                var transformNodes = refNode.SelectXmlNodes("ds:Transforms/ds:Transform");

                // Should be only one algorithm (Canonicalization - omit comments)
                foreach (XmlNode transformNode in transformNodes)
                {
                    string transformAlgorithm = transformNode.AtrValue("Algorithm");

                    if (transformAlgorithm != ValidationEnums.Canonicalization.CanonicalizationMethod)
                    {
                        return validationError.AppendErrorMessage($"Invalid transform algorithm : {transformAlgorithm}");
                    }

                    var outputArray = CanonicalizationHelper.CanonicalizeXmlDigest(manifestNode, ValidationEnums.HashAlgorithms.SHAMappings[digestAlgorithm]);
                    string digestOutputBase64String = Convert.ToBase64String(outputArray);

                    // Retrieve expected digest
                    string xmlDigestValueBase64String = refNode.SelectXmlNode("ds:DigestValue")?.InnerText;

                    if (digestOutputBase64String != xmlDigestValueBase64String)
                    {
                        return validationError.AppendErrorMessage("Digest values do not match.");
                    }
                }
            }

            return validationError;
        }

        [XadesTValidationHandler(ExecutionOrder: 2, Description: "Overenie hodnoty ds:SignatureValue pomocou pripojeného podpisového certifikátu v ds:KeyInfo")]
        public ValidationError SignatureValueVerificationHandler(XmlDocument xmlDoc, string xmlFileName)
        {
            ValidationError validationError = new ValidationError(xmlFileName, null);

            var signedInfoElement = xmlDoc.SelectXmlNode("//ds:Signature/ds:SignedInfo");
            var signatureMethodElement = xmlDoc.SelectXmlNode("//ds:Signature/ds:SignedInfo/ds:SignatureMethod");
            var canonicalizationMethodElement = xmlDoc.SelectXmlNode("//ds:Signature/ds:SignedInfo/ds:CanonicalizationMethod");
            var signatureValueElement = xmlDoc.SelectXmlNode("//ds:Signature/ds:SignatureValue");

            if (signatureValueElement == null) return validationError.AppendErrorMessage(nameof(signatureValueElement) + " missing");
            if (signatureMethodElement == null) return validationError.AppendErrorMessage(nameof(signatureMethodElement) + " missing");
            if (canonicalizationMethodElement == null) return validationError.AppendErrorMessage(nameof(canonicalizationMethodElement) + " missing");
            if (signedInfoElement == null) return validationError.AppendErrorMessage(nameof(signedInfoElement) + " missing");

            var certificate = XmlNodeHelper.GetX509Certificate(xmlDoc);

            if (certificate == null)
                return validationError.AppendErrorMessage("X509Certificate element is missing");

            var canMethod = canonicalizationMethodElement.AtrValue("Algorithm");

            if (canMethod != ValidationEnums.Canonicalization.CanonicalizationMethod)
                return validationError.AppendErrorMessage($"Not supported cannonicalization method. {canMethod}");

            var digestBytes = CanonicalizationHelper.CanonicalizeXml(signedInfoElement);

            string singnatureAlgorithm = signatureMethodElement.AtrValue("Algorithm");

            if (!ValidationEnums.Cryptography.SupportedSignatureSchemasMappings.ContainsKey(singnatureAlgorithm))
                return validationError.AppendErrorMessage($"Not supported signing algorithm {singnatureAlgorithm}");

            var signingAlgo = ValidationEnums.Cryptography.SupportedSignatureSchemasMappings[singnatureAlgorithm];

            AsymmetricKeyParameter publicKey = certificate.GetPublicKey();
            ISigner signer = SignerUtilities.GetSigner(signingAlgo);
            signer.Init(false, publicKey);
            signer.BlockUpdate(digestBytes, 0, digestBytes.Length);

            if (!signer.VerifySignature(Convert.FromBase64String(signatureValueElement.InnerText)))
            {
                return validationError.AppendErrorMessage("Cannot verify signature with publick key.");
            }

            return validationError;
        }
    }
}
