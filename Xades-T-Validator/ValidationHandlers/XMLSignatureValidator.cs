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
    [XadesTValidator(ValidationTaskName: "Overenie XML Signature")]
    public class XMLSignatureValidator : BaseXadesTValidator
    {
        public XMLSignatureValidator(IEnumerable<XMLDocumentWrapper> documentWrappers) : base(documentWrappers)
        {
        }

        [XadesTValidationHandler(ExecutionOrder: 1, Description: "CanonicalizationMethod musí obsahovať URI niektorého z podporovaných algoritmov pre dané elementy podľa profilu XAdES_ZEP")]
        public ValidationError ValidationHandler1(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            try
            {
                var canonicalizationMethod = xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature/ds:SignedInfo/ds:CanonicalizationMethod", GetNamespaceManager(xmlDoc));
                if (canonicalizationMethod.Attributes["Algorithm"].Value != "http://www.w3.org/TR/2001/REC-xml-c14n-20010315")
                    validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }
            catch (Exception /*ex*/)
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }

            return validationError;
        }

        [XadesTValidationHandler(ExecutionOrder: 1, Description: "SignatureMethod musí obsahovať URI niektorého z podporovaných algoritmov pre dané elementy podľa profilu XAdES_ZEP")]
        public ValidationError ValidationHandler2(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            List<String> signatureMethodAlgorithms = new List<string>()
            {
                "http://www.w3.org/2000/09/xmldsig#dsa-sha1", 
                "http://www.w3.org/2000/09/xmldsig#rsa-sha1",
                "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256",
                "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384",
                "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512"
            };

            try
            {
                var signatureMethod = xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature/ds:SignedInfo/ds:SignatureMethod", GetNamespaceManager(xmlDoc));
                if (!signatureMethodAlgorithms.Contains(signatureMethod.Attributes["Algorithm"].Value))
                    validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }
            catch (Exception /*ex*/)
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }

            return validationError;
        }
    }
}
