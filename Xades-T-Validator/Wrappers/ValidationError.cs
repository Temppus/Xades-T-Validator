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
            return FileName + " -> - " + ErrorMessage;
        }

        public void AppendErrorMessage(string message)
        {
            if (ErrorMessage == null)
            {
                ErrorMessage = message;
                return;
            }

            StringBuilder sb = new StringBuilder("\n\t");

            for (int i = 0; i < FileName.Length + 8; i++)
                sb.Append(" ");

            sb.Append("- ");
            sb.Append(message);

            ErrorMessage += sb.ToString();
        }
    }
}
