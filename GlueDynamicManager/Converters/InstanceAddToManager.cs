using FlatRedBall;
using FlatRedBall.Graphics;
using FlatRedBall.Math;
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
        public static void AddToManager(ObjectContainer objectContainer, List<ObjectContainer> otherObjects, Layer layer)
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
            else if (instance is Polygon asPolygon)
            {
                ShapeManager.AddPolygon(asPolygon);
            }
            else if (instance is Sprite asSprite)
            {
                SpriteManager.AddSprite(asSprite);
            }
            else if (instance is Text asText)
            {
                TextManager.AddText(asText);
            }
            else if (instance is LayeredTileMap asLayeredTileMap)
            {
                asLayeredTileMap.AddToManagers(layer);
            }
            else if (instance is TileShapeCollection asTileShapeCollection)
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

                var collisionFillLeft = GetProperty<float>("CollisionFillLeft");
                var collisionFillTop = GetProperty<float>("CollisionFillTop");

                var innerSizeWidth = GetProperty<float>("InnerSizeWidth");
                var innerSizeHeight = GetProperty<float>("InnerSizeHeight");


                var borderOutlineType = GetProperty<int>("BorderOutlineType");
                switch (creationOption)
                {
                    case 0: // empty
                        // do nothing
                        break;
                    case 1: // FillCompletely
                        asTileShapeCollection.GridSize = collisionTileSize;

                        asTileShapeCollection.LeftSeedX = collisionFillLeft % collisionTileSize;
                        asTileShapeCollection.BottomSeedY = collisionFillTop % collisionTileSize;

                        asTileShapeCollection.SortAxis = FlatRedBall.Math.Axis.X;

                        for (int x = 0; x < collisionFillWidth; x++)
                        {
                            for (int y = 0; y < collisionFillHeight; y++)
                            {
                                asTileShapeCollection.AddCollisionAtWorld(
                                    collisionFillLeft + x * collisionTileSize + collisionTileSize / 2.0f,
                                    collisionFillTop - y * collisionTileSize - collisionTileSize / 2.0f);
                            }

                        }
                        //int(int x = 0; x < width; x++)
                        //{

                        break;
                    case 2: // BorderOutline
                        asTileShapeCollection.GridSize = collisionTileSize;

                        asTileShapeCollection.LeftSeedX = collisionFillLeft % collisionTileSize;
                        asTileShapeCollection.BottomSeedY = collisionFillTop % collisionTileSize;

                        asTileShapeCollection.SortAxis = FlatRedBall.Math.Axis.X;

                        if (borderOutlineType == 1) // InnerSize
                        {

                            var additionalWidth = 2 * collisionTileSize;
                            var additionalHeight = 2 * collisionTileSize;

                            collisionFillWidth = MathFunctions.RoundToInt((innerSizeWidth + additionalWidth) / collisionTileSize);
                            collisionFillHeight = MathFunctions.RoundToInt((innerSizeHeight + additionalHeight) / collisionTileSize);
                        }

                        for(int x = 0; x < collisionFillWidth; x++)
                        {
                            if(x == 0 || x == collisionFillWidth - 1)
                            {
                                for (int y = 0; y < collisionFillHeight; y++)
                                {
                                    asTileShapeCollection.AddCollisionAtWorld(
                                        collisionFillLeft + x * collisionTileSize + collisionTileSize / 2.0f,
                                        collisionFillTop - y * collisionTileSize - collisionTileSize / 2.0f);
                                }
                            }
                            else
                            {
                                asTileShapeCollection.AddCollisionAtWorld(
                                    collisionFillLeft + x * collisionTileSize + collisionTileSize / 2.0f,
                                    collisionFillTop - collisionTileSize / 2.0f);

                                asTileShapeCollection.AddCollisionAtWorld(
                                    collisionFillLeft + x * collisionTileSize + collisionTileSize / 2.0f,
                                    collisionFillTop - (collisionFillHeight - 1) * collisionTileSize - collisionTileSize / 2.0f);
                            }
                        }


                        break;
                    case 4: // FromType

                        var sourceTmxName = GetProperty<string>("SourceTmxName");
                        var collisionTileTypeName = GetProperty<string>("CollisionTileTypeName");
                        var removeTilesAfterCreatingCollision = GetProperty<bool>("RemoveTilesAfterCreatingCollision");
                        var isCollisionMerged = GetProperty<bool>("IsCollisionMerged");

                        if (!string.IsNullOrEmpty(sourceTmxName) && !string.IsNullOrEmpty(collisionTileTypeName))
                        {
                            LayeredTileMap layeredTileMap = otherObjects.Find(item => item.Name == sourceTmxName)?.Value as LayeredTileMap;
                            if(isCollisionMerged)
                            {
                                FlatRedBall.TileCollisions.TileShapeCollectionLayeredTileMapExtensions.AddMergedCollisionFromTilesWithType(
                                    asTileShapeCollection, layeredTileMap, collisionTileTypeName);
                            }
                            else
                            {
                                FlatRedBall.TileCollisions.TileShapeCollectionLayeredTileMapExtensions.AddCollisionFromTilesWithType(
                                    asTileShapeCollection, layeredTileMap, collisionTileTypeName, removeTilesAfterCreatingCollision);
                            }

                        }

                        break;
                }
            }
            else if ((method = instance.GetType().GetMethods().Where(item => item.Name == "AddToManagers").FirstOrDefault()) != null)
            {
                method.Invoke(instance, new object[] { layer });
            }
            else
            {
                throw new NotImplementedException($"Need to handle {instance.GetType()}");
            }
        }
    }
}
