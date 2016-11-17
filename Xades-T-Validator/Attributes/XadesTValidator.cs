using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xades_T_Validator.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class XadesTValidator : Attribute
    {
        public string ValidationTaskName { get; set; }

        public XadesTValidator(string ValidationTaskName)
        {
            this.ValidationTaskName = ValidationTaskName;
        }
    }
}
