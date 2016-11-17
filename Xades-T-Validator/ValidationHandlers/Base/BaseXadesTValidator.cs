using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xades_T_Validator.Attributes;
using Xades_T_Validator.ValidationHandlers.Interfaces;
using Xades_T_Validator.Wrappers;

namespace Xades_T_Validator.ValidationHandlers.Base
{
    public abstract class BaseXadesTValidator : IValidationMessagesCollector
    {
        private IEnumerable<XMLDocumentWrapper> _documentWrappers;

        public BaseXadesTValidator(IEnumerable<XMLDocumentWrapper> documentWrappers)
        {
            _documentWrappers = documentWrappers;
        }


        public XmlNamespaceManager GetNamespaceManager(XmlDocument xmlDoc)
        {
            XmlNamespaceManager namespaces = new XmlNamespaceManager(xmlDoc.NameTable);

            namespaces.AddNamespace("xades", "http://uri.etsi.org/01903/v1.3.2#");
            namespaces.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            namespaces.AddNamespace("xzep", "http://www.ditec.sk/ep/signature_formats/xades_zep/v1.01");

            return namespaces;
        }

        public string GetErrorMessage(MethodBase methodbase)
        {
            XadesTValidationHandlerAttribute attr = (XadesTValidationHandlerAttribute)methodbase.GetCustomAttributes(typeof(XadesTValidationHandlerAttribute), true)[0];
            return attr.Description;
        }

        public IEnumerable<string> CollectValidationErrors()
        {
            List<string> validationMessages = new List<string>();

            foreach (var xmlWrapper in _documentWrappers)
            {
                var sortedmethods = FetchSortedValidationHandlers();

                foreach (var pair in sortedmethods)
                {
                    var attributes = pair.Value.GetCustomAttributes(typeof(XadesTValidationHandlerAttribute), true);
                    if (attributes != null && attributes.Length > 0)
                    {
                        object[] paramArray = new object[] { xmlWrapper };
                        ValidationError valError = (ValidationError)pair.Value.Invoke(this, paramArray);

                        if (valError.ErrorMessage != null)
                            validationMessages.Add(pair.Key + ".) " + valError.ToString());
                    }
                }
            }

            return validationMessages;
        }

        private SortedList<int, MethodInfo> FetchSortedValidationHandlers()
        {
            SortedList<int, MethodInfo> handlers = new SortedList<int, MethodInfo>();

            var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(XadesTValidationHandlerAttribute), true);
                if (attributes != null && attributes.Length > 0)
                {
                    XadesTValidationHandlerAttribute valAttr = (XadesTValidationHandlerAttribute)method.GetCustomAttributes(typeof(XadesTValidationHandlerAttribute), true)[0];
                    handlers.Add(valAttr.ExecutionOrder, method);
                }
            }
            return handlers;
        }
    }
}
