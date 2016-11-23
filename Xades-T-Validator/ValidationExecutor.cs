using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xades_T_Validator.Attributes;
using Xades_T_Validator.ValidationHandlers;
using Xades_T_Validator.ValidationHandlers.Base;
using Xades_T_Validator.ValidationHandlers.Interfaces;
using Xades_T_Validator.XMLHelpers;

namespace Xades_T_Validator
{
    public class ValidationExecutor
    {
        private static string XML_DIR_PATH = Directory.GetParent(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName).FullName).FullName;
        private static string VALIDATION_ERRORS_FILE_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "validationErrors.txt");

        static void Main(string[] args)
        {
            XML_DIR_PATH = Path.Combine(XML_DIR_PATH, "Documents");

            var xmlDocs = XmlLoaderHelper.LoadXMLDocuments(XML_DIR_PATH);

            if (File.Exists(VALIDATION_ERRORS_FILE_PATH))
                File.Delete(VALIDATION_ERRORS_FILE_PATH);

            var validators = FetchSortedValidators();

            foreach (var validatorItem in validators)
            {
                var type = validatorItem.Value;

                IValidationMessagesCollector errorCollector = (IValidationMessagesCollector)Activator.CreateInstance(type, xmlDocs);

                using (StreamWriter sw = File.AppendText(VALIDATION_ERRORS_FILE_PATH))
                {
                    XadesTValidator valAttr = (XadesTValidator)type.GetCustomAttributes(typeof(XadesTValidator), true)[0];
                    sw.WriteLine("[" + valAttr.ExecutionOrder + "]");
                    sw.WriteLine("###### " + valAttr.ValidationTaskName + " ######\n");

                    foreach (var valError in errorCollector.CollectValidationErrors())
                    {
                        sw.WriteLine("\t" + valError);
                    }
                }
            }
        }

        private static SortedList<int, Type> FetchSortedValidators()
        {
            SortedList<int, Type> validators = new SortedList<int, Type>();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.GetCustomAttributes(typeof(XadesTValidator), true).Length > 0)
                {
                    XadesTValidator valAttr = (XadesTValidator)type.GetCustomAttributes(typeof(XadesTValidator), true)[0];
                    validators.Add(valAttr.ExecutionOrder, type);
                }
            }
            return validators;
        }
    }
}
