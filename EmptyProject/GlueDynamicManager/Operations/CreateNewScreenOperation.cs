using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmptyProject.GlueDynamicManager.Operations
{
    internal class CreateNewScreenOperation : IOperation
    {
        private JToken value;

        public CreateNewScreenOperation(JToken value)
        {
            this.value = value;
        }
    }
}
