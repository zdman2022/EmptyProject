using FlatRedBall;
using FlatRedBall.Graphics;
using FlatRedBall.Math;
using FlatRedBall.Math.Geometry;
using GlueDynamicManager.DynamicInstances.Containers;
using Gum.Wireframe;
using GumCoreShared.FlatRedBall.Embedded;
using System;
using System.Collections;
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

        internal void Destroy()
        {
            foreach(var item in InstancedObjects)
            {
                if(item.Value is AxisAlignedRectangle rectangle)
                {
                    ShapeManager.Remove(rectangle);
                }else if(item.Value is Circle circle)
                {
                    ShapeManager.Remove(circle);
                }else if(item.Value is Polygon polygon)
                {
                    ShapeManager.Remove(polygon);
                }else if(item.Value is Sprite sprite)
                {
                    SpriteManager.RemoveSprite(sprite);
                }else if(item.Value is Text text)
                {
                    TextManager.RemoveText(text);
                }else if(item.Value.GetType().IsGenericType && item.Value.GetType().GetGenericTypeDefinition() == typeof(PositionedObjectList<>))
                {
                    var list = (IList)item.Value;
                    foreach(PositionedObject po in list)
                    {
                        po.RemoveSelfFromListsBelongingTo();
                    }
                }else if(item.Value is GraphicalUiElement element)
                {
                    element.Destroy();
                }else if(item.Value is PositionedObjectGueWrapper gueWrapper)
                {
                    gueWrapper.RemoveSelfFromListsBelongingTo();
                }
            }
        }
    }
}
