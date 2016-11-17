using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xades_T_Validator.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class XadesTValidationHandlerAttribute : Attribute
    {
        public string Description { get; set; }
        public int ExecutionOrder { get; set; }

        public XadesTValidationHandlerAttribute(int ExecutionOrder, string Description)
        {
            this.Description = Description;
            this.ExecutionOrder = ExecutionOrder;
        }
    }
}
