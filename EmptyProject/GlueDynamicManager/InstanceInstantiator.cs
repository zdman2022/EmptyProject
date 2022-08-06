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
    internal class InstanceInstantiator
    {
        internal static object Instantiate(string sourceClassType)
        {
            if (sourceClassType == typeof(AxisAlignedCube).FullName)
            {
                return new AxisAlignedCube();
            }
            if (sourceClassType == typeof(AxisAlignedRectangle).FullName)
            {
                return new AxisAlignedRectangle();
            }
            else if(sourceClassType == typeof(Circle).FullName)
            {
                return new Circle();
            }
            else if (sourceClassType == typeof(Line).FullName)
            {
                return new Line();
            }
            else if (sourceClassType == typeof(Polygon).FullName)
            {
                return new Polygon();
            }
            else if (sourceClassType == typeof(Sphere).FullName)
            {
                return new Sphere();
            }
            else if (sourceClassType == typeof(Sprite).FullName)
            {
                return new Sprite();
            }
            else if (sourceClassType == typeof(Text).FullName)
            {
                return new Text();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
