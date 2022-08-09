using FlatRedBall;
using FlatRedBall.Graphics;
using FlatRedBall.Math.Geometry;
using FlatRedBall.TileCollisions;
using FlatRedBall.TileGraphics;
using GlueControl.Managers;
using GlueDynamicManager.DynamicInstances.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GlueDynamicManager
{
    internal class InstanceAddToManager
    {
        public static void AddToManager(ObjectContainer objectContainer, Layer layer)
        {
            var instance = objectContainer.Value;

            MethodInfo method;

            if (instance is AxisAlignedCube aaCube)
            {
                ShapeManager.AddAxisAlignedCube(aaCube);
            }
            else if (instance is AxisAlignedRectangle aaRect)
            {
                ShapeManager.AddAxisAlignedRectangle(aaRect);
            }
            else if (instance is Circle asCircle)
            {
                ShapeManager.AddCircle(asCircle);
            }
            else if(instance is Polygon asPolygon)
            {
                ShapeManager.AddPolygon(asPolygon);
            }
            else if(instance is Sprite asSprite)
            {
                SpriteManager.AddSprite(asSprite);
            }
            else if(instance is Text asText)
            {
                TextManager.AddText(asText);
            }
            else if(instance is LayeredTileMap asLayeredTileMap)
            {
                asLayeredTileMap.AddToManagers(layer);
            }
            else if(instance is TileShapeCollection asTileShapeCollection)
            {
                // for now we assume it's visible. Eventually we look at the NOS
                // we may not need to do this, as it's done automatically
                //var value = ObjectFinder.Self.GetValueRecursively(
                //    objectContainer.NamedObjectSave,
                //    ObjectFinder.Self.GetElementContaining(objectContainer.NamedObjectSave),
                //    "Visible");

                //if(value is bool asBool && asBool)
                //{
                //    asTileShapeCollection.Visible = true;
                //}

                var nos = objectContainer.NamedObjectSave;
                T GetProperty<T>(string name) =>
                    ObjectFinder.Self.GetPropertyValueRecursively<T>(nos, name);


                var creationOption = GetProperty<int>("CollisionCreationOptions");
                var collisionTileSize = GetProperty<float>("CollisionTileSize");
                var collisionFillWidth = GetProperty<float>("CollisionFillWidth");
                var collisionFillHeight = GetProperty<float>("CollisionFillHeight");
                var innerSizeWidth = GetProperty<float>("InnerSizeWidth");
                var innerSizeHeight = GetProperty<float>("InnerSizeHeight");

                switch(creationOption)
                {
                    case 0: // empty
                        // do nothing
                        break;
                    case 1: // FillCompletely

                        break;
                    case 2: // BorderOutline

                        break;
                }
            }    
            else if((method = instance.GetType().GetMethods().Where(item => item.Name == "AddToManagers").FirstOrDefault()) != null)
            {
                method.Invoke(instance, new object[] { layer });
            }
            else if(instance.GetType().IsGenericType && 
                (
                    instance.GetType().GetGenericTypeDefinition() == typeof(FlatRedBall.Math.PositionedObjectList<>)
                    ||
                    instance.GetType().GetGenericTypeDefinition() == typeof(FlatRedBall.Math.Collision.AlwaysCollidingListCollisionRelationship<>)
                    ||
                    instance.GetType().GetGenericTypeDefinition() == typeof(FlatRedBall.Math.Collision.CollidableListVsTileShapeCollectionRelationship<>)
                    ||
                    instance.GetType().GetGenericTypeDefinition() == typeof(FlatRedBall.Math.Collision.ListVsListRelationship<,>)
                )
            )
            {
                //Do nothing
            }
            else
            {
                throw new NotImplementedException($"Need to handle {instance.GetType()}");
            }
        }
    }
}
