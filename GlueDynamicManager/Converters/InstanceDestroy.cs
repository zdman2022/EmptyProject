using FlatRedBall;
using FlatRedBall.Graphics;
using FlatRedBall.Instructions;
using FlatRedBall.Math;
using FlatRedBall.Math.Geometry;
using FlatRedBall.TileCollisions;
using FlatRedBall.TileGraphics;
using GlueDynamicManager.DynamicInstances;
using GlueDynamicManager.DynamicInstances.Containers;
using System;
using System.Collections;

namespace GlueDynamicManager.Converters
{
    internal class InstanceDestroy
    {
        public static void Destroy(ObjectContainer objectContainer)
        {
            Destroy(objectContainer.Value);
        }

        public static void Destroy(object instance)
        {
            Action body = () =>
            {
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
                else if (instance is IDestroyable asDestroyable)
                {
                    asDestroyable.Destroy();
                }
                else if (instance is AxisAlignedCube aaCube)
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
                else if(instance is TileShapeCollection asTileShapeCollection)
                {
                    asTileShapeCollection.RemoveFromManagers(); // full removal, do we want to one-way it?
                }
                else if(instance is IDynamic dynamic)
                {
                    dynamic.Destroy();
                }
                else
                {
                    TypeHandler.CallMethodIfExists(instance, "Destroy", new object[] { }, out var methodReturnValue);
                }
            };

            if (FlatRedBallServices.IsThreadPrimary())
                body();
            else
                InstructionManager.DoOnMainThreadAsync(body).Wait();
        }
    }
}
