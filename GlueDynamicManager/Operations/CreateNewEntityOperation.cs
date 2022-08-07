using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlueDynamicManager.Operations
{
    internal class CreateNewEntityOperation : IOperation
    {
        private JToken value;

        public CreateNewEntityOperation(JToken value)
        {
            this.value = value;
        }
    }
}
