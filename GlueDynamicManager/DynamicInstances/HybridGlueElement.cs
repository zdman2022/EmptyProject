using FlatRedBall;
using FlatRedBall.Graphics;
using FlatRedBall.Instructions;
using FlatRedBall.Math;
using FlatRedBall.Math.Geometry;
using GlueControl.Models;
using GlueDynamicManager.DynamicInstances.Containers;
using Gum.Wireframe;
using GumCoreShared.FlatRedBall.Embedded;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                RemoveObject(item.Value);
            }
        }

        private void RemoveObject(object obj, bool skipThreadCheck = false)
        {
            Action body = () =>
            {
                if (obj is AxisAlignedRectangle rectangle)
                {
                    ShapeManager.Remove(rectangle);
                }
                else if (obj is Circle circle)
                {
                    ShapeManager.Remove(circle);
                }
                else if (obj is Polygon polygon)
                {
                    ShapeManager.Remove(polygon);
                }
                else if (obj is Sprite sprite)
                {
                    SpriteManager.RemoveSprite(sprite);
                }
                else if (obj is Text text)
                {
                    TextManager.RemoveText(text);
                }
                else if (obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(PositionedObjectList<>))
                {
                    var list = (IList)obj;
                    foreach (PositionedObject po in list)
                    {
                        po.RemoveSelfFromListsBelongingTo();
                    }
                }
                else if (obj is GraphicalUiElement element)
                {
                    element.Destroy();
                }
                else if (obj is PositionedObjectGueWrapper gueWrapper)
                {
                    gueWrapper.RemoveSelfFromListsBelongingTo();
                }
            };

            if (FlatRedBallServices.IsThreadPrimary())
                body();
            else
                InstructionManager.DoOnMainThreadAsync(body).Wait();
        }

        internal void RemoveNamedObject(NamedObjectSave removeNO)
        {
            RemoveObject(PropertyFinder(removeNO.InstanceName));
        }
    }
}
