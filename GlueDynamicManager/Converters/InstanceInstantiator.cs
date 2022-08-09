using FlatRedBall;
using FlatRedBall.Graphics;
using FlatRedBall.Math.Geometry;
using FlatRedBall.TileGraphics;
using GlueControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlueDynamicManager
{
    internal class InstanceInstantiator
    {
        public static readonly string[] IgnoreAssemblies = new[]
        {
            "mscorlib",
            "MonoGame.Framework",
            "System",
            "System.Core",
            "FlatRedBallDesktopGL",
            "JsonDiffPatchDotNet",
            "System.Xml",
            "Newtonsoft.Json",
            "System.Numerics",
            "System.Runtime.Serialization",
            "System.Data",
            "GumCoreXnaPc",
            "System.Configuration",
            "Microsoft.GeneratedCode",
            "FlatRedBall.Forms",
            "Microsoft.VisualStudio.Debugger.Runtime.Desktop"
        };

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
            else if(sourceClassType == typeof(FlatRedBall.TileCollisions.TileShapeCollection).FullName)
            {
                return new FlatRedBall.TileCollisions.TileShapeCollection();
            }
            else
            {
                throw new NotImplementedException($"Need to handle instantiation for type {sourceClassType}");
            }
        }

        internal static object InstantiateEntity(string sourceType)
        {
            var foundType = AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !IgnoreAssemblies.Any(ignoreItem => assembly.FullName.StartsWith(ignoreItem))).SelectMany(assembly => assembly.DefinedTypes.Where(subType => subType.Name == sourceType)).FirstOrDefault();

            var instance = Activator.CreateInstance(foundType);

            return instance;
        }
    }
}
