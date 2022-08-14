using GlueControl.Models;
using GlueDynamicManager.Converters;
using GlueDynamicManager.DynamicInstances.Containers;
using System.Collections.Generic;
using System.Linq;

namespace GlueDynamicManager.DynamicInstances
{
    internal class HybridGlueElement
    {
        public List<ObjectContainer> InstancedObjects = new List<ObjectContainer>();

        public HybridGlueElement(object element)
        {
            GlueElement = element;
        }

        public object GlueElement { get; set; }

        public object PropertyFinder(string name)
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

        internal void Destroy()
        {
            foreach(var item in InstancedObjects)
            {
                InstanceDestroy.Destroy(item);
            }
        }
        internal void RemoveNamedObject(NamedObjectSave removeNO)
        {
            InstanceDestroy.Destroy(PropertyFinder(removeNO.InstanceName));
        }
    }
}
