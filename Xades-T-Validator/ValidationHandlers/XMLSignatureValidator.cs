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

namespace Xades_T_Validator.ValidationHandlers
{
    [XadesTValidator(ExecutionOrder: 2, ValidationTaskName: "Overenie XML Signature")]
    public class XMLSignatureValidator : BaseXadesTValidator
    {
        public XMLSignatureValidator(IEnumerable<XMLDocumentWrapper> documentWrappers) : base(documentWrappers)
        {
        }

        [XadesTValidationHandler(ExecutionOrder: 1, Description: "CanonicalizationMethod musí obsahovať URI niektorého z podporovaných algoritmov podľa profilu XAdES_ZEP")]
        public ValidationError ValidationHandler1(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            var canonicalizationMethod = xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature/ds:SignedInfo/ds:CanonicalizationMethod", xmlDoc.NameSpaceManager());

            if (canonicalizationMethod.Attributes["Algorithm"]?.Value != "http://www.w3.org/TR/2001/REC-xml-c14n-20010315")
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());

            return validationError;
        }

        [XadesTValidationHandler(ExecutionOrder: 2, Description: "SignatureMethod musí obsahovať URI niektorého z podporovaných algoritmov podľa profilu XAdES_ZEP")]
        public ValidationError ValidationHandler2(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            List<string> signatureMethodAlgorithms = new List<string>()
            {
                "http://www.w3.org/2000/09/xmldsig#dsa-sha1",
                "http://www.w3.org/2000/09/xmldsig#rsa-sha1",
                "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256",
                "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384",
                "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512"
            };

            var signatureMethod = xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature/ds:SignedInfo/ds:SignatureMethod", xmlDoc.NameSpaceManager());

            if (!signatureMethodAlgorithms.Contains(signatureMethod.Attributes["Algorithm"]?.Value))
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());

            return validationError;
        }

        [XadesTValidationHandler(ExecutionOrder: 3, Description: "Transform musí obsahovať URI niektorého z podporovaných algoritmov podľa profilu XAdES_ZEP")]
        public ValidationError ValidationHandler3(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            var transforms = xmlDoc.DocumentElement.SelectNodes("//ds:Signature/ds:SignedInfo/ds:Reference/ds:Transforms/ds:Transform", xmlDoc.NameSpaceManager());

            for (int i = 0; i < transforms.Count; i++)
            {
                if (transforms[i].Attributes["Algorithm"]?.Value != "http://www.w3.org/TR/2001/REC-xml-c14n-20010315")
                {
                    validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
                    break;
                }
            }

            return validationError;
        }

        [XadesTValidationHandler(ExecutionOrder: 4, Description: "DigestMethod musí obsahovať URI niektorého z podporovaných algoritmov podľa profilu XAdES_ZEP")]
        public ValidationError ValidationHandler4(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            List<string> digestMethodAlgorithms = new List<string>()
            {
                    "http://www.w3.org/2000/09/xmldsig#sha1",
                    "http://www.w3.org/2001/04/xmldsig-more#sha224",
                    "http://www.w3.org/2001/04/xmlenc#sha256",
                    "http://www.w3.org/2001/04/xmldsig-more#sha384",
                    "http://www.w3.org/2001/04/xmlenc#sha512"
            };

            var digestMethods = xmlDoc.DocumentElement.SelectNodes("//ds:Signature/ds:SignedInfo/ds:Reference/ds:DigestMethod", xmlDoc.NameSpaceManager());

            for (int i = 0; i < digestMethods.Count; i++)
            {
                if (!digestMethodAlgorithms.Contains(digestMethods[i].Attributes["Algorithm"]?.Value))
                    validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }

            return validationError;
        }
    }
}
