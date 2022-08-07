using FlatRedBall.Content.Instructions;
using GlueControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlueDynamicManager.DynamicInstances.Containers
{
    internal abstract class BaseContainer<T> : IInstanceContainer
    {
        public NamedObjectSave NamedObjectSave { get; set; }
        public string Name => NamedObjectSave?.InstanceName;
        public T Value { get; set; }
        public List<InstructionSave> CombinedInstructionSaves { get; internal set; }

        public object GetValue()
        {
            return Value;
        }

        public override string ToString() => $"{NamedObjectSave.InstanceName} ({Value?.GetType()})";
    }
}
