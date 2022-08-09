using FlatRedBall;
using FlatRedBall.Graphics;
using FlatRedBall.Math.Geometry;
using FlatRedBall.TileGraphics;
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
        public static void AddToManager(object instance, Layer layer)
        {
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
            else if((method = instance.GetType().GetMethods().Where(item => item.Name == "AddToManagers").FirstOrDefault()) != null)
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
