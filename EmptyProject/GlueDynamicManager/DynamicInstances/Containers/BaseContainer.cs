using FlatRedBall.Content.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmptyProject.GlueDynamicManager.DynamicInstances.Containers
{
    internal abstract class BaseContainer<T> : IInstanceContainer
    {
        public string Name { get; set; }
        public bool AddToManagers { get; set; }
        public T Value { get; set; }
        public List<InstructionSave> InstructionSaves { get; internal set; }

        public object GetValue()
        {
            return Value;
        }

        public override string ToString() => $"{Name} ({Value?.GetType()})";
    }
}
