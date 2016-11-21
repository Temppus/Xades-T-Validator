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
using Xades_T_Validator.Enums;

namespace Xades_T_Validator.ValidationHandlers
{
    [XadesTValidator(ExecutionOrder: 2, ValidationTaskName: "Overenie XML Signature")]
    public class XMLSignatureValidator : BaseXadesTValidator
    {
        public XMLSignatureValidator(IEnumerable<XMLDocumentWrapper> documentWrappers) : base(documentWrappers)
        {
        }

        [XadesTValidationHandler(ExecutionOrder: 1, Description: "CanonicalizationMethod musí obsahovať URI niektorého z podporovaných algoritmov podľa profilu XAdES_ZEP")]
        public ValidationError ValidationHandler1(XmlDocument xmlDoc, string xmlFileName)
        {
            ValidationError validationError = new ValidationError(xmlFileName, null);
            
            var canonicalizationMethod = xmlDoc.SelectXmlNode("//ds:Signature/ds:SignedInfo/ds:CanonicalizationMethod");

            if (canonicalizationMethod.AtrValue("Algorithm") != ValidationEnums.Canonicalization.CanonicalizationMethod)
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());

            return validationError;
        }

        [XadesTValidationHandler(ExecutionOrder: 2, Description: "SignatureMethod musí obsahovať URI niektorého z podporovaných algoritmov podľa profilu XAdES_ZEP")]
        public ValidationError ValidationHandler2(XmlDocument xmlDoc, string xmlFileName)
        {
            ValidationError validationError = new ValidationError(xmlFileName, null);
            
            var signatureMethod = xmlDoc.SelectXmlNode("//ds:Signature/ds:SignedInfo/ds:SignatureMethod");

            if (!ValidationEnums.Cryptography.SupportedSignatureSchemasMappings.ContainsKey(signatureMethod.AtrValue("Algorithm")))
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());

            return validationError;
        }

        [XadesTValidationHandler(ExecutionOrder: 3, Description: "Transform musí obsahovať URI niektorého z podporovaných algoritmov podľa profilu XAdES_ZEP")]
        public ValidationError ValidationHandler3(XmlDocument xmlDoc, string xmlFileName)
        {
            ValidationError validationError = new ValidationError(xmlFileName, null);
            
            var transformNodes = xmlDoc.SelectXmlNodes("//ds:Signature/ds:SignedInfo/ds:Reference/ds:Transforms/ds:Transform");

            foreach (XmlNode transform in transformNodes)
            {
                if (transform.AtrValue("Algorithm") != ValidationEnums.Canonicalization.CanonicalizationMethod)
                {
                    validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
                    break;
                }
            }

            return validationError;
        }

        [XadesTValidationHandler(ExecutionOrder: 4, Description: "DigestMethod musí obsahovať URI niektorého z podporovaných algoritmov podľa profilu XAdES_ZEP")]
        public ValidationError ValidationHandler4(XmlDocument xmlDoc, string xmlFileName)
        {
            ValidationError validationError = new ValidationError(xmlFileName, null);
            
            var digestMethods = xmlDoc.SelectXmlNodes("//ds:Signature/ds:SignedInfo/ds:Reference/ds:DigestMethod");

            foreach (XmlNode digestMethod in digestMethods)
            {
                if (!ValidationEnums.HashAlgorithms.SHAMappings.ContainsKey(digestMethod.AtrValue("Algorithm")))
                    validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }

            return validationError;
        }
    }
}
