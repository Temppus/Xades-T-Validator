﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xades_T_Validator.Attributes;
using Xades_T_Validator.ValidationHandlers.Base;
using Xades_T_Validator.Wrappers;

namespace Xades_T_Validator.ValidationHandlers
{
    [XadesTValidator(ExecutionOrder: 4, ValidationTaskName: "Overenie ostatných elementov profilu XAdES_ZEP, ktoré prináležia do špecifikácie XML Signature")]
    partial class OtherElementsValidation : BaseXadesTValidator
    {
        private const string xmlnsDs = "http://www.w3.org/2000/09/xmldsig#";
        private Dictionary<string, string> refElementsInfos;

        public OtherElementsValidation(IEnumerable<XMLDocumentWrapper> documentWrappers) : base(documentWrappers)
        {
            refElementsInfos = new Dictionary<string, string>();
            refElementsInfos.Add("ds:KeyInfo", "http://www.w3.org/2000/09/xmldsig#Object");
            refElementsInfos.Add("ds:SignatureProperties", "http://www.w3.org/2000/09/xmldsig#SignatureProperties");
            refElementsInfos.Add("xades:SignedProperties", "http://uri.etsi.org/01903#SignedProperties");
            refElementsInfos.Add("ds:Manifest", "http://www.w3.org/2000/09/xmldsig#Manifest");
        }

        #region Signature-SignatureValue-SignedInfo

        [XadesTValidationHandler(ExecutionOrder: 1, Description: "ds:Signature: musí mať Id atribút, musí mať špecifikovaný namespace xmlns:ds")]
        public ValidationError ValidateSignature(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            if (xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature", GetNamespaceManager(xmlDoc)).Attributes["Id"] == null)
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }
            if (xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature", GetNamespaceManager(xmlDoc)).Attributes["xmlns:ds"].Value != xmlnsDs)
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

            if (xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature/ds:SignatureValue", GetNamespaceManager(xmlDoc)).Attributes["Id"] == null)
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

            XmlNodeList references = xmlDoc.DocumentElement.SelectNodes("//ds:Signature/ds:SignedInfo/ds:Reference", GetNamespaceManager(xmlDoc));

            foreach (XmlElement xmlRef in references)
            {
                string refType = xmlRef.Attributes["Type"]?.Value;
                string targetId = xmlRef.Attributes["URI"]?.Value?.Substring(1);
                var targetElement = xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature//*[@Id='" + targetId + "']", GetNamespaceManager(xmlDoc));

                if (refType == null || targetId == null || targetElement == null)
                {
                    validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
                    return validationError;
                }

                if (!refElementsInfos.Contains(new KeyValuePair<string, string>(targetElement.Name, refType)))
                {
                    validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
                }
            }

            if (xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature/ds:SignedInfo/ds:Reference[@Type='http://www.w3.org/2000/09/xmldsig#Object']", GetNamespaceManager(xmlDoc)) == null)
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }
            else if (xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature/ds:SignedInfo/ds:Reference[@Type='http://www.w3.org/2000/09/xmldsig#SignatureProperties']", GetNamespaceManager(xmlDoc)) == null)
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }
            else if (xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature/ds:SignedInfo/ds:Reference[@Type='http://uri.etsi.org/01903#SignedProperties']", GetNamespaceManager(xmlDoc)) == null)
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

            if (xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature/ds:Object/ds:SignatureProperties", GetNamespaceManager(xmlDoc)).Attributes["Id"] == null)
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }
            XmlNode signatureVersion = xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature/ds:Object/ds:SignatureProperties/ds:SignatureProperty/xzep:SignatureVersion", GetNamespaceManager(xmlDoc));
            XmlNode productInfos = xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature/ds:Object/ds:SignatureProperties/ds:SignatureProperty/xzep:ProductInfos", GetNamespaceManager(xmlDoc));
            if (signatureVersion == null || productInfos == null)
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
                return validationError;
            }

            string signatureId = xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature", GetNamespaceManager(xmlDoc))?.Attributes["Id"]?.Value;
            if (signatureVersion.ParentNode.Attributes["Target"].Value.Substring(1) != signatureId || productInfos.ParentNode.Attributes["Target"].Value.Substring(1) != signatureId)
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }

            return validationError;
        }
        #endregion

        #region KeyInfoValidation
        [XadesTValidationHandler(ExecutionOrder: 4, Description: "-------KeyInfo musí mať ID atribút")]
        public ValidationError ValidationHandler2_1(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            //check keyInfo id attribute
            var keyInfoIdAttribute = xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature/ds:KeyInfo", GetNamespaceManager(xmlDoc))?.Attributes["Id"];

            if (keyInfoIdAttribute == null)
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());

            //check x509
            var x509Data = xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature/ds:KeyInfo/ds:X509Data", GetNamespaceManager(xmlDoc));

            if (x509Data == null)
                validationError.AppendErrorMessage("KeyInfo musí obsahovať element x509Data");
            else if(x509Data.SelectSingleNode("//ds:X509Certificate", GetNamespaceManager(xmlDoc)) == null)
                validationError.AppendErrorMessage("x509Data musí obsahovať element x509Certificate");
            else if (x509Data.SelectSingleNode("//ds:X509IssuerSerial", GetNamespaceManager(xmlDoc)) == null)
                validationError.AppendErrorMessage("x509Data musí obsahovať element X509IssuerSerial");
            else if (x509Data.SelectSingleNode("//ds:X509SubjectName", GetNamespaceManager(xmlDoc)) == null)
                validationError.AppendErrorMessage("x509Data musí obsahovať element X509SubjectName");


            return validationError;
        }
        #endregion

        #region ManifestValidation

        [XadesTValidationHandler(
            ExecutionOrder: 6,
            Description: "overenie ds:Manifest elementov:" + 
                            "každý ds: Manifest element musí mať Id atribút," + 
                            "ds: Transforms musí byť z množiny podporovaných algoritmov pre daný element podľa profilu XAdES_ZEP," + 
                            "ds: DigestMethod – musí obsahovať URI niektorého z podporovaných algoritmov podľa profilu XAdES_ZEP," + 
                            "overenie hodnoty Type atribútu voči profilu XAdES_ZEP," + 
                            "každý ds: Manifest element musí obsahovať práve jednu referenciu na ds: Object")]
        public ValidationError ValidateManifestElement(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            XmlNodeList manifests = xmlDoc.DocumentElement.SelectNodes("//ds:Signature/ds:Object/ds:Manifest", GetNamespaceManager(xmlDoc));
            
            return validationError;
        }
        #endregion
    }
}
