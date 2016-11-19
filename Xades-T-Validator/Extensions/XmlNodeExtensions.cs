using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Xades_T_Validator.Extensions
{
    public static class XmlNodeExtensions
    {
        public static string AtrValue(this XmlNode xmlNode, string atrName)
        {
            return xmlNode.Attributes[atrName]?.Value;
        }
    }
}
