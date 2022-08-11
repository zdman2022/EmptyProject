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
            var prop = GlueElement.GetType().GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (prop != null)
                return prop.GetValue(GlueElement);

            prop = GlueElement.GetType().BaseType.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (prop != null)
                return prop.GetValue(GlueElement);

            //Search Fields
            var field = GlueElement.GetType().GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (field != null)
                return field.GetValue(GlueElement);

            field = GlueElement.GetType().BaseType.GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (field != null)
                return field.GetValue(GlueElement);

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
