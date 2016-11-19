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

namespace Xades_T_Validator.ValidationHandlers
{
    [XadesTValidator(ExecutionOrder: 4, ValidationTaskName: "Overenie ostatných elementov profilu XAdES_ZEP, ktoré prináležia do špecifikácie XML Signature")]
    partial class OtherElementsValidation : BaseXadesTValidator
    {
        private const string xmlnsDs = "http://www.w3.org/2000/09/xmldsig#";
        private const string refType = "http://www.w3.org/2000/09/xmldsig#Object";

        public OtherElementsValidation(IEnumerable<XMLDocumentWrapper> documentWrappers) : base(documentWrappers)
        {
        }

        #region Signature-SignatureValue-SignedInfo

        [XadesTValidationHandler(ExecutionOrder: 1, Description: "ds:Signature: musí mať Id atribút, musí mať špecifikovaný namespace xmlns:ds")]
        public ValidationError ValidateSignature(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            if (!xmlDoc.SelectXmlNode("//ds:Signature").AtrExists("Id"))
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }
            if (xmlDoc.SelectXmlNode("//ds:Signature").AtrValue("xmlns:ds") != xmlnsDs)
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }

            return validationError;
        }

        [XadesTValidationHandler(ExecutionOrder: 2, Description: "ds:SignatureValue – musí mať Id atribút")]
        public ValidationError ValidateSignatureValue(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            if (!xmlDoc.SelectXmlNode("//ds:Signature/ds:SignatureValue").AtrExists("Id"))
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }

            return validationError;
        }

        [XadesTValidationHandler(
            ExecutionOrder: 3, 
            Description: "overenie existencie referencií v ds:SignedInfo a hodnôt atribútov Id a Type voči profilu XAdES_ZEP pre: " + 
                            "ds:KeyInfo element, " + 
                            "ds: SignatureProperties element, " + 
                            "xades: SignedProperties element, " + 
                            "všetky ostatné referencie v rámci ds: SignedInfo musia byť referenciami na ds: Manifest elementy")]
        public ValidationError ValidateReferences(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            XmlNodeList references = xmlDoc.SelectXmlNodes("//ds:Signature/ds:SignedInfo/ds:Reference");

            foreach (XmlElement xmlRef in references)
            {
                string refType = xmlRef.AtrValue("Type");
                string targetId = xmlRef.AtrValue("URI")?.Substring(1);
                var targetElement = xmlDoc.SelectXmlNode("//ds:Signature//*[@Id='" + targetId + "']");

                if (refType == null || targetId == null || targetElement == null)
                {
                    validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
                    return validationError;
                }

                if (!ValidationEnums.ReferenceTypeConstraints.Mappings.Contains(new KeyValuePair<string, string>(targetElement.Name, refType)))
                {
                    validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
                }
            }

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
            }

            return validationError;
        }

        [XadesTValidationHandler(
            ExecutionOrder: 5, 
            Description: "overenie obsahu ds:SignatureProperties:" + 
                            "musí mať Id atribút," + 
                            "musí obsahovať dva elementy ds: SignatureProperty pre xzep: SignatureVersion a xzep: ProductInfos," + 
                            "obidva ds: SignatureProperty musia mať atribút Target nastavený na ds: Signature, -oba kontrola cez mriežku  #signatureid")]
        public ValidationError ValidateSignatureProperties(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            if (!xmlDoc.SelectXmlNode("//ds:Signature/ds:Object/ds:SignatureProperties").AtrExists("Id"))
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }
            XmlNode signatureVersion = xmlDoc.SelectXmlNode("//ds:Signature/ds:Object/ds:SignatureProperties/ds:SignatureProperty/xzep:SignatureVersion");
            XmlNode productInfos = xmlDoc.SelectXmlNode("//ds:Signature/ds:Object/ds:SignatureProperties/ds:SignatureProperty/xzep:ProductInfos");
            if (signatureVersion == null || productInfos == null)
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
                return validationError;
            }

            string signatureId = xmlDoc.SelectXmlNode("//ds:Signature")?.AtrValue("Id");
            if (signatureVersion.ParentNode.AtrValue("Target").Substring(1) != signatureId || productInfos.ParentNode.AtrValue("Target").Substring(1) != signatureId)
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }

            return validationError;
        }
        #endregion

        #region KeyInfoValidation
        [XadesTValidationHandler(ExecutionOrder: 4, Description: "KeyInfo musí mať ID atribút")]
        public ValidationError ValidationHandler1(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            var keyInfoID = xmlDoc.SelectXmlNode("//ds:Signature/ds:KeyInfo")?.AtrValue("Id");

            if(keyInfoID == null)
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());

            //check x509
            var x509Data = xmlDoc.SelectXmlNode("//ds:Signature/ds:KeyInfo/ds:X509Data");

            if (x509Data == null)
            {
                validationError.AppendErrorMessage("KeyInfo musí obsahovať element x509Data");
                return validationError;
            }

            //check certificate element
            if (x509Data.SelectXmlNode("ds:X509Certificate") == null)
            {
                validationError.AppendErrorMessage("x509Data musí obsahovať element x509Certificate");
                return validationError;
            }

            X509Certificate certificate =  XmlNodeHelper.GetX509Certificate(docWrapper);

            //check SubjectName
            if (x509Data.SelectXmlNode("ds:X509SubjectName") == null)
                validationError.AppendErrorMessage("x509Data musí obsahovať element SubjectName");
            else 
            {
                var X509SubjectName = x509Data.SelectXmlNode("ds:X509SubjectName");

                if (X509SubjectName.InnerText != certificate.CertificateStructure.Subject.ToString())
                    validationError.AppendErrorMessage("X509SubjectName sa nezhoduje");
            }

            //check IssuerSerial
            if (x509Data.SelectXmlNode("ds:X509IssuerSerial") == null)
                validationError.AppendErrorMessage("x509Data musí obsahovať element IssuerSerial");
            else
            {
                var X509IssuerSerial = x509Data.SelectXmlNode("ds:X509IssuerSerial/ds:X509IssuerName");

                X509Name xmlName = new X509Name(X509IssuerSerial.InnerText.Replace("S=", "ST="));

                if (!xmlName.Equivalent(certificate.CertificateStructure.Issuer))
                    validationError.AppendErrorMessage("IssuerSerial sa nezhoduje");
            }

            //check SerialNumber
            var X509SerialNumber = x509Data.SelectXmlNode("ds:X509IssuerSerial/ds:X509SerialNumber");

            if(X509SerialNumber == null)
                validationError.AppendErrorMessage("x509Data musí obsahovať element SerialNumber");
            else if (X509SerialNumber.InnerText != certificate.SerialNumber.ToString())
                validationError.AppendErrorMessage("X509SerialNumber sa nezhoduje");

            return validationError;
        }
        #endregion

        #region ManifestValidation

        [XadesTValidationHandler(
            ExecutionOrder: 6,
            Description: "overenie ds:Manifest elementov:" + 
                            "každý ds:Manifest element musí mať Id atribút," + 
                            "ds:Transforms musí byť z množiny podporovaných algoritmov pre daný element podľa profilu XAdES_ZEP," + 
                            "ds:DigestMethod – musí obsahovať URI niektorého z podporovaných algoritmov podľa profilu XAdES_ZEP," + 
                            "overenie hodnoty Type atribútu voči profilu XAdES_ZEP," + 
                            "každý ds:Manifest element musí obsahovať práve jednu referenciu na ds: Object")]
        public ValidationError ValidateManifestElement(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            XmlNodeList manifests = xmlDoc.SelectXmlNodes("//ds:Signature/ds:Object/ds:Manifest");
            foreach (XmlNode manifest in manifests)
            {
                //Manifest/references - count validation (must have one)
                XmlNodeList manifestReferences = manifest.SelectXmlNodes("ds:Reference");
                if (manifestReferences.Count != 1)
                {
                    validationError.AppendErrorMessage("overenie ds:Manifest elementov: každý ds:Manifest element musí obsahovať práve jednu referenciu na ds: Object");
                }

                //Id validation
                if (!manifest.AtrExists("Id"))
                {
                    validationError.AppendErrorMessage("overenie ds:Manifest elementov: každý ds:Manifest element musí mať Id atribút");
                }

                //Manifest/reference/transforms/transform - Algorithm validation
                XmlNodeList manifestTransforms = manifest.SelectXmlNodes("ds:Reference/ds:Transforms/ds:Transform");
                foreach (XmlNode transform in manifestTransforms)
                {
                    if (!ValidationEnums.ManifestTransformation.SupportedTransformations.Contains(transform.AtrValue("Algorithm")))
                    {
                        validationError.AppendErrorMessage("overenie ds:Manifest elementov: ds:Transforms musí byť z množiny podporovaných algoritmov pre daný element podľa profilu XAdES_ZEP,");
                    }
                }

                //Manifest/reference/digestMethod - Algoritm validation
                XmlNodeList manifestDigestMethods = manifest.SelectXmlNodes("ds:Reference/ds:DigestMethod");
                foreach (XmlNode digestMethod in manifestDigestMethods)
                {
                    if (!ValidationEnums.HashAlgorithms.SHAMappings.ContainsKey(digestMethod.AtrValue("Algorithm")))
                    {
                        validationError.AppendErrorMessage("overenie ds:Manifest elementov: ds:DigestMethod – musí obsahovať URI niektorého z podporovaných algoritmov podľa profilu XAdES_ZEP");
                    }
                }

                //Manifest/reference - Type validation
                XmlNode manifestReference = manifest.SelectXmlNode("ds:Reference");
                if(manifestReference.AtrValue("Type") != refType)
                {
                    validationError.AppendErrorMessage("overenie ds:Manifest elementov: overenie hodnoty Type atribútu voči profilu XAdES_ZEP");
                }

            }
            return validationError;
        }
        #endregion
    }
}
