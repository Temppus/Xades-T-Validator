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
    [XadesTValidator(ValidationTaskName: "Overenie ostatných elementov profilu XAdES_ZEP, ktoré prináležia do špecifikácie XML Signature")]
    class OtherElementsValidation : BaseXadesTValidator
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

        [XadesTValidationHandler(ExecutionOrder: 1, Description: "overenie existencie referencií v ds:SignedInfo a hodnôt atribútov Id a Type voči profilu XAdES_ZEP pre: ds:KeyInfo element, ds: SignatureProperties element, xades: SignedProperties element, všetky ostatné referencie v rámci ds: SignedInfo musia byť referenciami na ds: Manifest elementy")]
        public ValidationError ValidationHandler3(XMLDocumentWrapper docWrapper)
        {
            ValidationError validationError = new ValidationError(docWrapper.XmlName, null);
            XmlDocument xmlDoc = docWrapper.XmlDoc;

            try
            {
                XmlNodeList references = xmlDoc.DocumentElement.SelectNodes("//ds:Signature/ds:SignedInfo/ds:Reference", GetNamespaceManager(xmlDoc));
                foreach(XmlElement xmlRef in references){
                    string targetId = xmlRef.Attributes["URI"].Value.Substring(1);
                    var targetElement = xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature//*[@Id='" + targetId + "']", GetNamespaceManager(xmlDoc));
                    if (targetElement == null)
                    {
                        validationError.ErrorMessage = GetErrorMessage(MethodBase.GetCurrentMethod());
                    }
                    /*foreach (KeyValuePair<string, string> entry in refElementsInfos)
                    {
                        if ()
                        {
                            if ()
                            {

                            }
                        }
                    }
                    string targetNodeName = targetElement.Name;

                    // Check if reference is for supported elements only
                    if (REFERENCE_TYPES.containsKey(targetNodeName))
                    {
                        // Check if reference has correct type
                        if (!REFERENCE_TYPES.get(targetNodeName).equals(type))
                        {
                            throw new SignVerificationException(String.format("Element 'ds:Reference' has invalid 'Type' value - '%s' for element '%s' (Rule 9-12).",
                                    type, targetNodeName));
                        }
                    }
                    else
                    {
                        throw new SignVerificationException(String.format("Found reference to unsupported element '%s' (Rule 12).", targetNodeName));
                    }*/

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
