using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xades_T_Validator.Wrappers;

namespace Xades_T_Validator.XMLHelpers
{
    public static class XmlLoaderHelper
    {
        public static IEnumerable<XMLDocumentWrapper> LoadXMLDocuments(string fromPath)
        {
            IList<string> xmlFileNames = new DirectoryInfo(fromPath).GetFiles().OrderBy(f => f.Name).Select(f => f.Name).ToList();

            List<XMLDocumentWrapper> xmlDocs = new List<XMLDocumentWrapper>(xmlFileNames.Count);

            foreach (var fileName in xmlFileNames)
            {
                var xmldoc = new XmlDocument();
                xmldoc.PreserveWhitespace = true;
                xmldoc.Load(Path.Combine(fromPath, fileName));
                xmlDocs.Add(new XMLDocumentWrapper(xmldoc, fileName));
            }

            return xmlDocs;
        }
    }
}
