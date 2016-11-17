﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xades_T_Validator.Attributes;
using Xades_T_Validator.ValidationHandlers.Base;
using Xades_T_Validator.Wrappers;

namespace Xades_T_Validator.ValidationHandlers
{
    [XadesTValidator(ExecutionOrder: 6, ValidationTaskName: "Overenie platnosti podpisového certifikátu")]
    public class CertificatVerification : BaseXadesTValidator
    {
        public CertificatVerification(IEnumerable<XMLDocumentWrapper> documentWrappers) : base(documentWrappers)
        {
        }

    }
}
