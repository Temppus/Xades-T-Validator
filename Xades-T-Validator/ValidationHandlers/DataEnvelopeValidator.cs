using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Xades_T_Validator.Attributes;
using Xades_T_Validator.ValidationHandlers.Base;
using Xades_T_Validator.Wrappers;
using Xades_T_Validator.Extensions;

namespace Xades_T_Validator.ValidationHandlers
{
    [XadesTValidator(ExecutionOrder: 1, ValidationTaskName : "Overenie dátovej obálky")]
    public class DataEnvelopeValidator : BaseXadesTValidator
    {
        public DataEnvelopeValidator(IEnumerable<XMLDocumentWrapper> documents) : base(documents)
        {
        }

        [XadesTValidationHandler(ExecutionOrder : 1, Description: "Koreňový element musí obsahovať atribúty xmlns:xzep podľa profilu XADES_ZEP")]
        public ValidationError ValidationHandler1(XmlDocument xmlDoc, string xmlFileName)
        {
            ValidationError validationError = new ValidationError(xmlFileName, null);
            
            string zepURI = xmlDoc.DocumentElement.AtrValue("xmlns:xzep");

            if (zepURI != "http://www.ditec.sk/ep/signature_formats/xades_zep/v1.0")
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());

            return validationError;
        }

        [XadesTValidationHandler(ExecutionOrder: 2, Description: "Koreňový element musí obsahovať atribúty xmlns:ds podľa profilu XADES_ZEP")]
        public ValidationError ValidationHandler2(XmlDocument xmlDoc, string xmlFileName)
        {
            ValidationError validationError = new ValidationError(xmlFileName, null);
            
            string dsURI = xmlDoc.DocumentElement.AtrValue("xmlns:ds");

            if (dsURI != "http://www.w3.org/2000/09/xmldsig#")
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());

            return validationError;
        }
    }
}
