using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Xades_T_Validator.Attributes;
using Xades_T_Validator.ValidationHandlers.Base;
using Xades_T_Validator.Wrappers;

namespace Xades_T_Validator.ValidationHandlers
{
    [XadesTValidator(ValidationTaskName : "Overenie dátovej obálky")]
    public class DataEnvelopeValidator : BaseXadesTValidator
    {
        public DataEnvelopeValidator(IEnumerable<XMLDocumentWrapper> documents) : base(documents)
        {
        }

        [XadesTValidationHandler(ExecutionOrder : 1, Description: "Koreňový element musí obsahovať atribúty xmlns:xzep podľa profilu XADES_ZEP")]
        public ValidationError ValidationHandler1(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            try
            {
                if (xmlDoc.DocumentElement.Attributes["xmlns:xzep"].Value != "http://www.ditec.sk/ep/signature_formats/xades_zep/v1.0")
                    validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }
            catch (Exception /*ex*/)
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }

            return validationError;
        }

        [XadesTValidationHandler(ExecutionOrder: 1, Description: "Koreňový element musí obsahovať atribúty xmlns:ds podľa profilu XADES_ZEP")]
        public ValidationError ValidationHandler2(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            try
            {
                if (xmlDoc.DocumentElement.Attributes["xmlns:ds"].Value != "http://www.w3.org/2000/09/xmldsig#")
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
