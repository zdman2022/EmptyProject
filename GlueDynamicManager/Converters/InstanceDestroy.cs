using FlatRedBall;
using FlatRedBall.Graphics;
using FlatRedBall.Math;
using FlatRedBall.Math.Geometry;
using FlatRedBall.TileGraphics;
using GlueDynamicManager.DynamicInstances.Containers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GlueDynamicManager.Converters
{
    internal class InstanceDestroy
    {
        public static void Destroy(ObjectContainer objectContainer)
        {
            var nos = objectContainer.NamedObjectSave;
            ////////////////Early Out/////////////////////
            if (nos.InstantiatedByBase)
            {
                return;
            }
            /////////////End Early Out///////////////////

            var instance = objectContainer.Value;
            if (instance.GetType().IsGenericType && instance.GetType().GetGenericTypeDefinition() == typeof(PositionedObjectList<>))
            {

                instance.GetType().GetMethod("MakeOneWay").Invoke(instance, new object[] { });
                var enumerable = instance as IEnumerable;
                foreach (var item in enumerable)
                {
                    item.GetType().GetMethod("Destroy").Invoke(item, new object[] { });
                }
                instance.GetType().GetMethod("MakeTwoWay").Invoke(instance, new object[] { });
            }
            else if(instance is IDestroyable asDestroyable)
            {
                asDestroyable.Destroy();
            }
            if (instance is AxisAlignedCube aaCube)
            {
                ShapeManager.Remove(aaCube);
            }
            else if (instance is AxisAlignedRectangle aaRect)
            {
                ShapeManager.Remove(aaRect);
            }
            else if (instance is Circle asCircle)
            {
                ShapeManager.Remove(asCircle);
            }
            else if (instance is Polygon asPolygon)
            {
                ShapeManager.Remove(asPolygon);
            }
            else if (instance is Sprite asSprite)
            {
                SpriteManager.RemoveSprite(asSprite);
            }
            else if (instance is Text asText)
            {
                TextManager.RemoveText(asText);
            }
            else if (instance is LayeredTileMap asLayeredTileMap)
            {
                asLayeredTileMap.Destroy();
            }
        }

    }
}
