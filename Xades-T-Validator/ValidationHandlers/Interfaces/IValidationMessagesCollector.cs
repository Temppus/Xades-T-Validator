using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xades_T_Validator.ValidationHandlers.Interfaces
{
    public interface IValidationMessagesCollector
    {
        SortedSet<string> CollectValidationErrors();
    }
}
