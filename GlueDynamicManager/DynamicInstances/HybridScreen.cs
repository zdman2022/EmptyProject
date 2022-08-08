using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlatRedBall.Screens;
using GlueDynamicManager.DynamicInstances.Containers;

namespace GlueDynamicManager.DynamicInstances
{
    internal class HybridScreen
    {
        public List<PositionedListContainer> PositionedObjectLists = new List<PositionedListContainer>();

        // Do we want a second list for entities?
        public List<DynamicEntityContainer> InstancedEntities = new List<DynamicEntityContainer>();
        public List<ObjectContainer> InstancedObjects = new List<ObjectContainer>();

        public HybridScreen(Screen screen)
        {
            Screen = screen;
        }

        public Screen Screen { get; set; }

        public object PropertyFinder(string name)
        {
            object foundItem;

            foundItem = PositionedObjectLists.Where(item => item.Name == name).FirstOrDefault();

            if (foundItem != null)
                return foundItem;

            foundItem = InstancedObjects.Where(item => item.Name == name).FirstOrDefault();

            if (foundItem != null)
                return foundItem;

            var prop = Screen.GetType().GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (prop != null)
                return prop.GetValue(Screen);

            prop = Screen.GetType().BaseType.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (prop != null)
                return prop.GetValue(Screen);

            return null;
        }
    }
}
