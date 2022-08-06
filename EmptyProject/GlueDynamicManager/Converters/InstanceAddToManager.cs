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
            if (instance is AxisAlignedRectangle aaRect)
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
        }
    }
}
