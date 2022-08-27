using FlatRedBall.Content.Instructions;
using GlueControl.Models;
using GlueDynamicManager.Converters;
using GlueDynamicManager.DynamicInstances.Containers;
using System.Collections.Generic;
using System.Linq;

namespace GlueDynamicManager.DynamicInstances
{
    internal class HybridGlueElement : IDynamic
    {
        public List<ObjectContainer> InstancedObjects = new List<ObjectContainer>();

        public HybridGlueElement(object element)
        {
            GlueElement = element;
        }

        public object GlueElement { get; set; }

        public virtual string TypeName => GlueElement.GetType().FullName;
        public bool IsLoaded { get; set; }

        public object GetPropertyValue(string name)
        {
            //Search Dynamic
            var foundItem = InstancedObjects.Where(item => item.Name == name).FirstOrDefault();

            if (foundItem != null)
                return foundItem.Value;


            //Search Properties
            if(TypeHandler.GetPropValueIfExists(GlueElement, name, out var value))
                return value;

            //Search Fields
            if (TypeHandler.GetFieldValueIfExists(GlueElement, name, out value))
                return value;

            return null;
        }

        public void Destroy()
        {
            foreach(var item in InstancedObjects)
            {
                InstanceDestroy.Destroy(item);
            }
        }

        public void SetPropertyValue(string name, object value, NamedObjectSave nos, List<InstructionSave> instructionSaves)
        {
            //Search Properties
            if (TypeHandler.SetPropValueIfExists(GlueElement, name, value))
                return;

            //Search Fields
            if (TypeHandler.SetFieldValueIfExists(GlueElement, name, value))
                return;

            //Search Dynamic
            var foundItem = InstancedObjects.Where(item => item.Name == name).FirstOrDefault();
            InstancedObjects.Remove(foundItem);

            if (value != null)
                InstancedObjects.Add(new ObjectContainer(name)
                {
                    Value = value,
                    NamedObjectSave = nos,
                    CombinedInstructionSaves = instructionSaves
                });
        }

        public bool CallMethodIfExists(string methodName, object[] args, out object returnValue)
        {
            return TypeHandler.CallMethodIfExists(GlueElement, methodName, args, out returnValue);
        }

        public bool Equals(HybridGlueElement other)
        => this == other || this.GlueElement == other.GlueElement;

        public override bool Equals(object obj)
            => this == obj || this.GlueElement == obj;

        public override int GetHashCode()
            => this.GlueElement.GetHashCode();
    }
}
