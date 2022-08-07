using FlatRedBall.Graphics;
using FlatRedBall.Math.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmptyProject.GlueDynamicManager.Converters
{
    internal class ShapeManagerHandler
    {
        internal static bool IsShape(string objectType)
        {
            if (objectType == typeof(AxisAlignedCube).FullName)
            {
                return true;
            }
            else if (objectType == typeof(AxisAlignedRectangle).FullName)
            {
                return true;
            }
            else if (objectType == typeof(Circle).FullName)
            {
                return true;
            }
            else if (objectType == typeof(Line).FullName)
            {
                return true;
            }
            else if (objectType == typeof(Polygon).FullName)
            {
                return true;
            }
            else if (objectType == typeof(Sphere).FullName)
            {
                return true;
            }

            return false;
        }

        internal static void AddToLayer(object value, Layer layerProvidedByContainer, string objectType)
        {
            if (objectType == typeof(AxisAlignedCube).FullName)
            {
                ShapeManager.AddToLayer((AxisAlignedCube)value, layerProvidedByContainer);
            }
            else if (objectType == typeof(AxisAlignedRectangle).FullName)
            {
                ShapeManager.AddToLayer((AxisAlignedRectangle)value, layerProvidedByContainer);
            }
            else if (objectType == typeof(Circle).FullName)
            {
                ShapeManager.AddToLayer((Circle)value, layerProvidedByContainer);
            }
            else if (objectType == typeof(Line).FullName)
            {
                ShapeManager.AddToLayer((Line)value, layerProvidedByContainer);
            }
            else if (objectType == typeof(Polygon).FullName)
            {
                ShapeManager.AddToLayer((Polygon)value, layerProvidedByContainer);
            }
            else if (objectType == typeof(Sphere).FullName)
            {
                ShapeManager.AddToLayer((Sphere)value, layerProvidedByContainer);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
