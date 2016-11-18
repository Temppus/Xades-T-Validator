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

        [XadesTValidationHandler(ExecutionOrder: 3, Description: "overenie existencie referencií v ds:SignedInfo a hodnôt atribútov Id a Type voči profilu XAdES_ZEP pre: ds:KeyInfo element, ds: SignatureProperties element, xades: SignedProperties element, všetky ostatné referencie v rámci ds: SignedInfo musia byť referenciami na ds: Manifest elementy")]
        public ValidationError ValidateReferences(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            XmlNodeList references = xmlDoc.DocumentElement.SelectNodes("//ds:Signature/ds:SignedInfo/ds:Reference", GetNamespaceManager(xmlDoc));

            if (references.Count == 0)
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
                return validationError;
            }

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
            ExecutionOrder: 4, 
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

            return validationError;
        }
        #endregion

        #region KeyInfoValidation

        #endregion
    }
}
