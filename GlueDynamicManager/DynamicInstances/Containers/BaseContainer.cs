using FlatRedBall.Content.Instructions;
using GlueControl.Models;
using System.Collections.Generic;

namespace GlueDynamicManager.DynamicInstances.Containers
{
    internal abstract class BaseContainer<T> : IInstanceContainer
    {
        protected string _name;

        public NamedObjectSave NamedObjectSave { get; set; }
        public List<InstructionSave> CombinedInstructionSaves { get; set; }
        public string Name => _name;
        public T Value { get; set; }

        public object GetValue()
        {
            return Value;
        }

        public override string ToString() => $"{_name} ({Value?.GetType()})";
    }
}
