using FlatRedBall.Content.Instructions;
using GlueControl.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GlueDynamicManager.DynamicInstances
{
    internal interface IDynamic : IEquatable<object>
    {
        string TypeName { get; }
        object GetPropertyValue(string name);
        void SetPropertyValue(string name, object value, NamedObjectSave nos, List<InstructionSave> instructionSaves);
        bool CallMethodIfExists(string methodName, object[] args, out object returnValue);

        void Destroy();
    }
}
