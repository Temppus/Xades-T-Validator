using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xades_T_Validator.Extensions;

namespace Xades_T_Validator.Helpers
{
    public static class CanonicalizationHelper
    {
        public static byte[] CanonicalizeXml(XmlNode xmlNode)
        {
            XmlDocument manifestDoc = new XmlDocument();
            manifestDoc.PreserveWhitespace = true;
            manifestDoc.LoadXml(xmlNode.OuterXml);

            XmlDsigC14NTransform c14n = new XmlDsigC14NTransform(false);
            c14n.LoadInput(manifestDoc);

            return ((MemoryStream)c14n.GetOutput()).ToArray();
        }

        public static byte[] CanonicalizeXmlDigest(XmlNode xmlNode, System.Security.Cryptography.HashAlgorithm hashAlgo)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.LoadXml(xmlNode.OuterXml);

            XmlDsigC14NTransform c14n = new XmlDsigC14NTransform(false);
            c14n.LoadInput(xmlDoc);

            return c14n.GetDigestedOutput(hashAlgo);
        }
    }
}
