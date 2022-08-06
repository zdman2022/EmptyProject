using FlatRedBall;
using FlatRedBall.Graphics;
using FlatRedBall.Math.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmptyProject.GlueDynamicManager
{
    internal class InstanceAddToManager
    {
        public static void AddToManager(object instance)
        {
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
        }
    }
}
