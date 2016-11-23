using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xades_T_Validator.Attributes;
using Xades_T_Validator.ValidationHandlers.Base;
using Xades_T_Validator.Wrappers;
using Xades_T_Validator.Extensions;
using Xades_T_Validator.XMLHelpers;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1;
using Xades_T_Validator.Enums;
using System.IO;
using System.Security.Cryptography.Xml;
using Xades_T_Validator.Helpers;
using System.Security.Cryptography;

namespace Xades_T_Validator.ValidationHandlers
{
    [XadesTValidator(ExecutionOrder: 4, ValidationTaskName: "Overenie ostatných elementov profilu XAdES_ZEP, ktoré prináležia do špecifikácie XML Signature")]
    partial class OtherElementsValidation : BaseXadesTValidator
    {
        public OtherElementsValidation(IEnumerable<XMLDocumentWrapper> documentWrappers) : base(documentWrappers)
        {
        }

        #region Signature-SignatureValue-References

        [XadesTValidationHandler(ExecutionOrder: 1, Description: "ds:Signature: musí mať Id atribút, musí mať špecifikovaný namespace xmlns:ds")]
        public ValidationError ValidateSignature(XmlDocument xmlDoc, string xmlFileName)
        {
            ValidationError validationError = new ValidationError(xmlFileName, null);

            var signatureNode = xmlDoc.SelectXmlNode("//ds:Signature");

            if (signatureNode == null)
                return validationError.AppendErrorMessage("Missing signature element.");

            if (!signatureNode.AtrExists("Id"))
            {
                return validationError.AppendErrorMessage("Signature Id attribute missing or empty.");
            }
            else if (signatureNode.AtrValue("xmlns:ds") != "http://www.w3.org/2000/09/xmldsig#")
            {
                return validationError.AppendErrorMessage("Signature xmlns:ds missing or not equal to http://www.w3.org/2000/09/xmldsig#");
            }

            return validationError;
        }

        [XadesTValidationHandler(ExecutionOrder: 2, Description: "ds:SignatureValue – musí mať Id atribút")]
        public ValidationError ValidateSignatureValue(XmlDocument xmlDoc, string xmlFileName)
        {
            ValidationError validationError = new ValidationError(xmlFileName, null);

            if (!xmlDoc.SelectXmlNode("//ds:Signature/ds:SignatureValue").AtrExists("Id"))
            {
                return validationError.AppendErrorMessage("SignatureValue Id attribute missing or empty.");
            }

            return validationError;
        }

        [XadesTValidationHandler(
            ExecutionOrder: 3,
            Description: "Overenie existencie referencií v ds:SignedInfo a hodnôt atribútov Id a Type voči profilu XAdES_ZEP pre: " +
                            "ds:KeyInfo element, " +
                            "ds: SignatureProperties element, " +
                            "xades: SignedProperties element, " +
                            "všetky ostatné referencie v rámci ds: SignedInfo musia byť referenciami na ds: Manifest elementy")]
        public ValidationError ValidateReferences(XmlDocument xmlDoc, string xmlFileName)
        {
            ValidationError validationError = new ValidationError(xmlFileName, null);

            XmlNodeList signedInfoRefs = xmlDoc.SelectXmlNodes("//ds:Signature/ds:SignedInfo/ds:Reference");

            foreach (XmlElement signedInfoRef in signedInfoRefs)
            {
                string refType = signedInfoRef.AtrValue("Type");
                string refURI = signedInfoRef.AtrValue("URI")?.Substring(1);
                XmlNode referencedEle = xmlDoc.SelectXmlNode($"//ds:Signature//*[@Id='{refURI}']");

                if (referencedEle == null)
                {
                    return validationError.AppendErrorMessage($"Referenced with Id {refURI} does not exists.");
                }

                if (!ValidationEnums.ReferenceTypeConstraints.Mappings.Contains(new KeyValuePair<string, string>(referencedEle.Name, refType)))
                {
                    return validationError.AppendErrorMessage("Referenced not exists or type is not supported.");
                }
            }
            /*
            if (xmlDoc.SelectXmlNode("//ds:Signature/ds:SignedInfo/ds:Reference[@Type='http://www.w3.org/2000/09/xmldsig#Object']") == null)
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }
            else if (xmlDoc.SelectXmlNode("//ds:Signature/ds:SignedInfo/ds:Reference[@Type='http://www.w3.org/2000/09/xmldsig#SignatureProperties']") == null)
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }
            else if (xmlDoc.SelectXmlNode("//ds:Signature/ds:SignedInfo/ds:Reference[@Type='http://uri.etsi.org/01903#SignedProperties']") == null)
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }*/

            return validationError;
        }

        #endregion

        #region KeyInfoValidation

        [XadesTValidationHandler(ExecutionOrder: 4, Description: "KeyInfo musí mať ID atribút")]
        public ValidationError ValidationHandler1(XmlDocument xmlDoc, string xmlFileName)
        {
            ValidationError validationError = new ValidationError(xmlFileName, null);

            var keyInfoEle = xmlDoc.SelectXmlNode("//ds:Signature/ds:KeyInfo");

            if (keyInfoEle == null || !keyInfoEle.AtrExists("Id"))
                return validationError.AppendErrorMessage("KeyInfo does not exists or missing Id attribute.");

            // check x509
            var x509Data = xmlDoc.SelectXmlNode("//ds:Signature/ds:KeyInfo/ds:X509Data");

            if (x509Data == null)
            {
                return validationError.AppendErrorMessage("KeyInfo musí obsahovať element x509Data");
            }

            if (x509Data.SelectXmlNode("ds:X509Certificate") == null)
            {
                return validationError.AppendErrorMessage("x509Data musí obsahovať element x509Certificate");
            }

            X509Certificate certificate = XmlNodeHelper.GetX509Certificate(xmlDoc);

            //check SubjectName
            if (x509Data.SelectXmlNode("ds:X509SubjectName") == null)
                return validationError.AppendErrorMessage("x509Data musí obsahovať element SubjectName");
            else
            {
                var X509SubjectName = x509Data.SelectXmlNode("ds:X509SubjectName");

                if (X509SubjectName.InnerText != certificate.CertificateStructure.Subject.ToString())
                    return validationError.AppendErrorMessage("X509SubjectName sa nezhoduje");
            }

            //check IssuerSerial
            if (x509Data.SelectXmlNode("ds:X509IssuerSerial") == null)
                validationError.AppendErrorMessage("x509Data musí obsahovať element IssuerSerial");
            else
            {
                var X509IssuerSerial = x509Data.SelectXmlNode("ds:X509IssuerSerial/ds:X509IssuerName");

                X509Name xmlName = new X509Name(X509IssuerSerial.InnerText.Replace("S=", "ST="));

                if (!xmlName.Equivalent(certificate.CertificateStructure.Issuer))
                    return validationError.AppendErrorMessage("IssuerSerial sa nezhoduje");
            }

            //check SerialNumber
            var X509SerialNumber = x509Data.SelectXmlNode("ds:X509IssuerSerial/ds:X509SerialNumber");

            if (X509SerialNumber == null)
                return validationError.AppendErrorMessage("x509Data musí obsahovať element SerialNumber");
            else if (X509SerialNumber.InnerText != certificate.SerialNumber.ToString())
                return validationError.AppendErrorMessage("X509SerialNumber sa nezhoduje");

            return validationError;
        }

        #endregion

        [XadesTValidationHandler(
    ExecutionOrder: 5,
    Description: "Overenie obsahu ds:SignatureProperties:" +
                    "musí mať Id atribút," +
                    "musí obsahovať dva elementy ds: SignatureProperty pre xzep: SignatureVersion a xzep: ProductInfos," +
                    "obidva ds:SignatureProperty musia mať atribút Target nastavený na ds:Signature, -oba kontrola cez mriežku  #signatureid")]
        public ValidationError ValidateSignatureProperties(XmlDocument xmlDoc, string xmlFileName)
        {
            ValidationError validationError = new ValidationError(xmlFileName, null);

            if (!xmlDoc.SelectXmlNode("//ds:Signature/ds:Object/ds:SignatureProperties").AtrExists("Id"))
            {
                return validationError.AppendErrorMessage("Element SignatureProperties does not contain Id atribute.");
            }

            XmlNode signatureVersion = xmlDoc.SelectXmlNode("//ds:Signature/ds:Object/ds:SignatureProperties/ds:SignatureProperty/xzep:SignatureVersion");
            XmlNode productInfos = xmlDoc.SelectXmlNode("//ds:Signature/ds:Object/ds:SignatureProperties/ds:SignatureProperty/xzep:ProductInfos");

            if (signatureVersion == null || productInfos == null)
            {
                return validationError.AppendErrorMessage("Element SignatureVersion or ProductInfos missing in SignatureProperties.");
            }

            string signatureId = xmlDoc.SelectXmlNode("//ds:Signature")?.AtrValue("Id");
            if (signatureVersion.ParentNode.AtrValue("Target")?.Substring(1) != signatureId || productInfos.ParentNode.AtrValue("Target")?.Substring(1) != signatureId)
            {
                return validationError.AppendErrorMessage("obidva ds:SignatureProperty musia mať atribút Target nastavený na ds:Signature, -oba kontrola cez mriežku  #signatureid");
            }

            return validationError;
        }


        #region ManifestValidation

        [XadesTValidationHandler(
            ExecutionOrder: 6,
            Description: "overenie ds:Manifest elementov:" +
                            "každý ds:Manifest element musí mať Id atribút," +
                            "ds:Transforms musí byť z množiny podporovaných algoritmov pre daný element podľa profilu XAdES_ZEP," +
                            "ds:DigestMethod – musí obsahovať URI niektorého z podporovaných algoritmov podľa profilu XAdES_ZEP," +
                            "overenie hodnoty Type atribútu voči profilu XAdES_ZEP," +
                            "každý ds:Manifest element musí obsahovať práve jednu referenciu na ds: Object")]
        public ValidationError ValidateManifestElement(XmlDocument xmlDoc, string xmlFileName)
        {
            ValidationError validationError = new ValidationError(xmlFileName, null);

            XmlNodeList manifests = xmlDoc.SelectXmlNodes("//ds:Signature/ds:Object/ds:Manifest");
            foreach (XmlNode manifest in manifests)
            {
                // Manifest/references - count validation (must have one)
                XmlNodeList manifestReferences = manifest.SelectXmlNodes("ds:Reference");
                if (manifestReferences.Count != 1)
                {
                    return validationError.AppendErrorMessage("Overenie ds:Manifest elementov: každý ds:Manifest element musí obsahovať práve jednu referenciu na ds: Object");
                }

                // Id validation
                if (!manifest.AtrExists("Id"))
                {
                    return validationError.AppendErrorMessage("Overenie ds:Manifest elementov: každý ds:Manifest element musí mať Id atribút");
                }

                // Manifest/reference/transforms/transform - Algorithm validation
                XmlNodeList manifestTransforms = manifest.SelectXmlNodes("ds:Reference/ds:Transforms/ds:Transform");
                foreach (XmlNode transform in manifestTransforms)
                {
                    if (!ValidationEnums.ManifestTransformation.SupportedTransformations.Contains(transform.AtrValue("Algorithm")))
                    {
                        return validationError.AppendErrorMessage($"Overenie ds:Manifest elementov: ds:Transforms musí byť z množiny podporovaných algoritmov pre daný element podľa profilu XAdES_ZEP. {transform.AtrValue("Algorithm")}");
                    }
                }

                // Manifest/reference/digestMethod - Algoritm validation
                XmlNodeList manifestDigestMethods = manifest.SelectXmlNodes("ds:Reference/ds:DigestMethod");
                foreach (XmlNode digestMethod in manifestDigestMethods)
                {
                    if (!ValidationEnums.HashAlgorithms.SHAMappings.ContainsKey(digestMethod.AtrValue("Algorithm")))
                    {
                        return validationError.AppendErrorMessage("Overenie ds:Manifest elementov: ds:DigestMethod – musí obsahovať URI niektorého z podporovaných algoritmov podľa profilu XAdES_ZEP");
                    }
                }

                // Manifest/reference - Type validation
                XmlNode manifestReference = manifest.SelectXmlNode("ds:Reference");
                if (manifestReference.AtrValue("Type") != "http://www.w3.org/2000/09/xmldsig#Object")
                {
                    return validationError.AppendErrorMessage("Overenie ds:Manifest elementov: overenie hodnoty Type atribútu voči profilu XAdES_ZEP");
                }
            }
            return validationError;
        }

        [XadesTValidationHandler(
            ExecutionOrder: 7,
            Description: "overdManifest: " +
                            "dereferencovanie URI, aplikovanie príslušnej ds: Transforms transformácie(pri base64 decode)," +
                            "overenie hodnoty ds: DigestValue")]
        public ValidationError ValidateManifestReference(XmlDocument xmlDoc, string xmlFileName)
        {
            ValidationError validationError = new ValidationError(xmlFileName, null);

            XmlNodeList manifestReferences = xmlDoc.SelectXmlNodes("//ds:Signature/ds:Object/ds:Manifest/ds:Reference");
            foreach (XmlNode manifestRef in manifestReferences)
            {
                var refURI = manifestRef.AtrValue("URI")?.Substring(1);
                XmlNode referencedObject = xmlDoc.SelectXmlNode($"ds:Object[@Id='{refURI}']");

                if (referencedObject == null)
                    return validationError.AppendErrorMessage("Referenced object does not exist");

                byte[] referencedElementByte = CanonicalizationHelper.CanonicalizeXml(referencedObject);

                string refStr = Encoding.UTF8.GetString(referencedElementByte);

                XmlNodeList transforms = manifestRef.SelectXmlNodes("ds:Transforms/ds:Transform");
                string digestAlgo = manifestRef.SelectXmlNode("ds:DigestMethod")?.AtrValue("Algorithm");
                string digestOutputBase64String = null;

                foreach (XmlNode transformEle in transforms)
                {
                    string transformAlgo = transformEle.AtrValue("Algorithm");

                    if (transformAlgo == ValidationEnums.Canonicalization.CanonicalizationMethod)
                    {
                        var hashAlgo = ValidationEnums.HashAlgorithms.SHAMappings[digestAlgo];
                        var outputArray = CanonicalizationHelper.CanonicalizeXml(referencedObject);

                        SHA256 s = new SHA256Managed();

                        digestOutputBase64String = Convert.ToBase64String(s.ComputeHash(outputArray));
                    }
                    else if (transformAlgo == "http://www.w3.org/2000/09/xmldsig#base64")
                    {
                        digestOutputBase64String = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(referencedElementByte)));
                    }
                    else
                    {
                        return validationError.AppendErrorMessage($"Not supported transform algorithm :  {transformAlgo}");
                    }

                    string digestValue = manifestRef.SelectXmlNode("ds:DigestValue")?.InnerText;

                    if (digestValue != digestOutputBase64String)
                    {
                        return validationError.AppendErrorMessage($"ds:DigestValue values do not match {digestValue}  <-> {digestOutputBase64String}");
                    }
                    else
                        ;
                }
            }
            return validationError;
        }
        #endregion
    }
}
