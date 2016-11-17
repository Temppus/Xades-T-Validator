using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xades_T_Validator.Attributes;
using Xades_T_Validator.ValidationHandlers.Base;
using Xades_T_Validator.Wrappers;

namespace Xades_T_Validator.ValidationHandlers
{
    [XadesTValidator(ValidationTaskName: "Overenie ostatných elementov profilu XAdES_ZEP, ktoré prináležia do špecifikácie XML Signature")]
    class OtherElementsValidation : BaseXadesTValidator
    {
        public OtherElementsValidation(IEnumerable<XMLDocumentWrapper> documentWrappers) : base(documentWrappers)
        {
        }
    }
}
