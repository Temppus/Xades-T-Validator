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
    [XadesTValidator(ValidationTaskName: "Overenie ostatných elementov profilu XAdES_ZEP, ktoré prináležia do špecifikácie XML Signature")]
    class OtherElementsValidation : BaseXadesTValidator
    {
        private const string xmlnsDs = "http://www.w3.org/2000/09/xmldsig#";

        public OtherElementsValidation(IEnumerable<XMLDocumentWrapper> documentWrappers) : base(documentWrappers)
        {
        }

        #region Signature-SignatureValue-SignedInfo

        [XadesTValidationHandler(ExecutionOrder: 1, Description: "ds:Signature: musí mať Id atribút, musí mať špecifikovaný namespace xmlns:ds")]
        public ValidationError ValidationHandler1(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            try
            {
                if (xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature", GetNamespaceManager(xmlDoc)).Attributes["Id"] == null)
                {
                    validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
                }
                if (xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature", GetNamespaceManager(xmlDoc)).Attributes["xmlns:ds"].Value != xmlnsDs)
                {
                    validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
                }
            }
            catch (Exception /*ex*/)
            {
                validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
            }

            return validationError;
        }

        [XadesTValidationHandler(ExecutionOrder: 1, Description: "ds:SignatureValue – musí mať Id atribút")]
        public ValidationError ValidationHandler2(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            try
            {
                if (xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature/ds:SignatureValue", GetNamespaceManager(xmlDoc)).Attributes["Id"] == null)
                {
                    validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
                }
            }
            catch (Exception /*ex*/)
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
