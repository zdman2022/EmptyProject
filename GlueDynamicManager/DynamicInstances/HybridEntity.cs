using System.Collections.Generic;
using System.Linq;
using GlueDynamicManager.DynamicInstances.Containers;

namespace GlueDynamicManager.DynamicInstances
{
    internal class HybridEntity
    {
        public List<PositionedListContainer> PositionedObjectLists = new List<PositionedListContainer>();

        // Do we want a second list for entities?
        public List<DynamicEntityContainer> InstancedEntities = new List<DynamicEntityContainer>();
        public List<ObjectContainer> InstancedObjects = new List<ObjectContainer>();

        public HybridEntity(object entity)
        {
            Entity = entity;
        }

        public object Entity { get; set; }

        public object PropertyFinder(string name)
        {
            object foundItem;

            foundItem = PositionedObjectLists.Where(item => item.Name == name).FirstOrDefault();

            if (foundItem != null)
                return foundItem;

            foundItem = InstancedObjects.Where(item => item.Name == name).FirstOrDefault();

            if (foundItem != null)
                return foundItem;

            var prop = Entity.GetType().GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (prop != null)
                return prop.GetValue(Entity);

            prop = Entity.GetType().BaseType.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (prop != null)
                return prop.GetValue(Entity);

            return null;
        }
    }
}
