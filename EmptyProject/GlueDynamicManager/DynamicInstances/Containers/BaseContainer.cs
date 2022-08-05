using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmptyProject.GlueDynamicManager.DynamicInstances.Containers
{
    internal abstract class BaseContainer<T>
    {
        public string Name { get; set; }
        public bool AddToManagers { get; set; }
        public T Value { get; set; }
    }
}
