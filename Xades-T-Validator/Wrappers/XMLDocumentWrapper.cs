using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Xades_T_Validator.Wrappers
{
    public class XMLDocumentWrapper
    {
        public XmlDocument XmlDoc { get; set; }
        public string XmlName { get; set; }

        public XMLDocumentWrapper(XmlDocument doc, string fileName)
        {
            XmlDoc = doc;
            XmlName = fileName;
        }
    }
}
