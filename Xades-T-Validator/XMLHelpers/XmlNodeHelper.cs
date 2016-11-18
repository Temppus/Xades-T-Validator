﻿using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xades_T_Validator.Extensions;
using Xades_T_Validator.Wrappers;

namespace Xades_T_Validator.XMLHelpers
{
    public static class XmlNodeHelper
    {
        public static X509Certificate GetCertificate(XMLDocumentWrapper docWrapper)
        {
            XmlDocument xmlDoc = docWrapper.XmlDoc;
            var encodedCertificate = xmlDoc.DocumentElement.SelectSingleNode("//ds:Signature/ds:KeyInfo/ds:X509Data/ds:X509Certificate", xmlDoc.NameSpaceManager());

            var parser = new X509CertificateParser();
            byte[] certificateBytes = Convert.FromBase64String(encodedCertificate.InnerText);
            var bouncyCertificate = parser.ReadCertificate(certificateBytes);

            return bouncyCertificate;
        }
    }
}
