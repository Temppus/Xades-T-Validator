using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Xades_T_Validator.Extensions
{
    public static class XmlDocumentExtensions
    {
        public static XmlNamespaceManager NameSpaceManager(this XmlDocument xmlDoc)
        {
            XmlNamespaceManager namespaces = new XmlNamespaceManager(xmlDoc.NameTable);

            namespaces.AddNamespace("xades", "http://uri.etsi.org/01903/v1.3.2#");
            namespaces.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            namespaces.AddNamespace("xzep", "http://www.ditec.sk/ep/signature_formats/xades_zep/v1.01");
            namespaces.AddNamespace("xzep", "http://www.ditec.sk/ep/signature_formats/xades_zep/v1.0");

            return namespaces;
        }
    }
}
