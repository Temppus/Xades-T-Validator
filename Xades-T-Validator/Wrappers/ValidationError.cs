using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xades_T_Validator.Wrappers
{
    public class ValidationError
    {
        public string FileName { get; set; }
        public string ErrorMessage { get; set; }

        public ValidationError(string fileName, string errorMessage)
        {
            FileName = fileName;
            ErrorMessage = errorMessage;
        }

        public override string ToString()
        {
            return FileName + " -> " + ErrorMessage;
        }
    }
}
