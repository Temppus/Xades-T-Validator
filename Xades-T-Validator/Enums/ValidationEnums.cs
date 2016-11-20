using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Xades_T_Validator.Enums
{
    public class ValidationEnums
    {
        public class HashAlgorithms
        {
            /// <summary>
            /// Supported hash algorithms
            /// </summary>
            public static readonly Dictionary<string, System.Security.Cryptography.HashAlgorithm> SHAMappings = new Dictionary<string, System.Security.Cryptography.HashAlgorithm>
            {
                { "http://www.w3.org/2000/09/xmldsig#sha1", new SHA1CryptoServiceProvider() },
                { "http://www.w3.org/2001/04/xmldsig-more#sha224", null  }, // Missing in System.Security.Cryptography
                { "http://www.w3.org/2001/04/xmlenc#sha256", new SHA256CryptoServiceProvider() },
                { "http://www.w3.org/2001/04/xmldsig-more#sha384", new SHA384CryptoServiceProvider() },
                { "http://www.w3.org/2001/04/xmlenc#sha512", new SHA512CryptoServiceProvider() }
            };
        }

        public class Canonicalization
        {
            /// <summary>
            /// Only one transformation supported by XAdES_ZEP for manifest element
            /// </summary>
            public static readonly string CanonicalizationMethod = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
        }

        public class ManifestTransformation
        {
            /// <summary>
            /// Supported transformations for manifest
            /// </summary>
            public static readonly List<string> SupportedTransformations = new List<string>()
            {
                    "http://www.w3.org/TR/2001/REC-xml-c14n-20010315",
                    "http://www.w3.org/2000/09/xmldsig#base64"
            };
        }

        public class ReferenceTypeConstraints
        {
            /// <summary>
            /// Type constraints mappings for Reference elements
            /// </summary>
            public static readonly Dictionary<string, string> Mappings = new Dictionary<string, string>
            {
                { "ds:KeyInfo", "http://www.w3.org/2000/09/xmldsig#Object" },
                { "ds:SignatureProperties", "http://www.w3.org/2000/09/xmldsig#SignatureProperties"},
                { "xades:SignedProperties", "http://uri.etsi.org/01903#SignedProperties"},
                { "ds:Manifest", "http://www.w3.org/2000/09/xmldsig#Manifest"}
            };
        }

        public class Cryptography
        {
            /// <summary>
            /// Mappings of algortithm URI to algorithm name for XAdES_ZEP supported cryptography algorihtms of Signature
            /// </summary>
            public static readonly Dictionary<string, string> SupportedSignatureSchemasMappings = new Dictionary<string, string>
            {
                { "http://www.w3.org/2000/09/xmldsig#dsa-sha1", "SHA1withDSA"},
                { "http://www.w3.org/2000/09/xmldsig#rsa-sha1", "SHA1withRSA/ISO9796-2"},
                { "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256", "SHA256withRSA"},
                { "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384", "SHA384withRSA"},
                { "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512", "SHA512withRSA"}
            };
        }

    }
}
