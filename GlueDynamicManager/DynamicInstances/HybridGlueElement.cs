using GlueDynamicManager.DynamicInstances.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            var foundItem = InstancedObjects.Where(item => item.Name == name).FirstOrDefault();

            if (foundItem != null)
                return foundItem.Value;

            var prop = GlueElement.GetType().GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (prop != null)
                return prop.GetValue(GlueElement);

            prop = GlueElement.GetType().BaseType.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (prop != null)
                return prop.GetValue(GlueElement);

            return null;
        }
    }
}
