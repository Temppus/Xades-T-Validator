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

        public static XmlNode SelectXmlNode(this XmlDocument xmlDoc, string xPath)
        {
            return xmlDoc.SelectSingleNode(xPath, xmlDoc.NameSpaceManager());
        }

        public static XmlNode SelectXmlNode(this XmlNode xmlNode, string xPath)
        {
            return xmlNode.SelectSingleNode(xPath, xmlNode.OwnerDocument.NameSpaceManager());
        }


        public static XmlNodeList SelectXmlNodes(this XmlDocument xmlDoc, string xPath)
        {
            return xmlDoc.SelectNodes(xPath, xmlDoc.NameSpaceManager());
        }

        public static XmlNodeList SelectXmlNodes(this XmlNode xmlNode, string xPath)
        {
            return xmlNode.SelectNodes(xPath, xmlNode.OwnerDocument.NameSpaceManager());
        }

        public static string AtrValue(this XmlNode xmlNode, string atrName)
        {
            return xmlNode.Attributes[atrName]?.Value;
        }

        public static bool AtrExists(this XmlNode xmlNode, string atrName)
        {
            return xmlNode.Attributes[atrName] != null;
        }
    }
}
