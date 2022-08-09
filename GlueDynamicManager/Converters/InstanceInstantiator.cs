using FlatRedBall;
using FlatRedBall.Graphics;
using FlatRedBall.Math.Collision;
using FlatRedBall.Math.Geometry;
using FlatRedBall.TileGraphics;
using GlueControl.Models;
using GlueDynamicManager.DynamicInstances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public static readonly string[] FRBAssemblies = new[]
        {
            "FlatRedBallDesktopGL",
            "FlatRedBall.Forms",
        };

        private static readonly Regex GenericSingleTypeRegEx = new Regex("^(.*)<([^,]*)>$");
        private static readonly Regex GenericDoubleTypeRegEx = new Regex("^(.*)<([^,]*),([^,]*)>$");

        internal static object Instantiate(string sourceClassType, List<PropertySave> properties, object container)
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
            else if (GenericSingleTypeRegEx.IsMatch(sourceClassType))
            {
                var match = GenericSingleTypeRegEx.Match(sourceClassType);
                var typeName = match.Groups[1].Value.Trim();
                var genTypeName = match.Groups[2].Value.Trim();

                Type genType = GetTypeCheckForDynamic(genTypeName);

                var parmList = GetParmsForType(sourceClassType, container, properties);

                var instance = InstantiateTypeWith1Generic(GetType(typeName + "`1", true), genType, parmList);

                ApplyPropertiesToInstance(sourceClassType, instance, container, properties);

                return instance;
            }
            else if (GenericDoubleTypeRegEx.IsMatch(sourceClassType))
            {
                var match = GenericDoubleTypeRegEx.Match(sourceClassType);
                var typeName = match.Groups[1].Value.Trim();
                var genTypeName1 = match.Groups[2].Value.Trim();
                var genTypeName2 = match.Groups[3].Value.Trim();

                Type genType1 = GetTypeCheckForDynamic(genTypeName1);
                Type genType2 = GetTypeCheckForDynamic(genTypeName2);

                var parmList = GetParmsForType(sourceClassType, container, properties);

                var instance = InstantiateTypeWith2Generic(GetType(typeName + "`2", true), genType1, genType2, parmList);

                ApplyPropertiesToInstance(sourceClassType, instance, container, properties);

                return instance;
            }
            //FlatRedBall.Math.Collision.AlwaysCollidingListCollisionRelationship<Entities.Player>
            //else if (sourceClassType.StartsWith(typeof(FlatRedBall.Math.Collision.AlwaysCollidingListCollisionRelationship<>).FullName.Replace("`1", "")))
            //{
            //    var match = GenericSingleTypeRegEx.Match(sourceClassType);
            //    var genTypeName = match.Groups[1].Value;

            //    Type genType;
            //    if (genTypeName.StartsWith("Entities.") && GlueDynamicManager.Self.EntityIsDynamic(genTypeName.Replace("Entities.", "")))
            //    {
            //        genType = typeof(DynamicEntity);
            //    }
            //    else
            //    {
            //        genType = GetType(genTypeName);
            //    }

            //    var list = GlueDynamicManager.Self.GetProperty(container, (string)properties.First(item => item.Name == "FirstCollisionName").Value);

            //    return InstantiateTypeWith1Generic(typeof(FlatRedBall.Math.Collision.AlwaysCollidingListCollisionRelationship<>), genType, new object[] { list });
            //}

            else
            {
                throw new NotImplementedException($"Need to handle instantiation for type {sourceClassType}");
            }
        }

        private static Type GetTypeCheckForDynamic(string genTypeName)
        {
            Type genType;
            if (genTypeName.StartsWith("Entities.") && GlueDynamicManager.Self.EntityIsDynamic(genTypeName.Replace("Entities.", "")))
            {
                genType = typeof(DynamicEntity);
            }
            else
            {
                genType = GetType(genTypeName, true);
            }

            return genType;
        }

        private static void ApplyPropertiesToInstance(string sourceClassType, object instance, object container, List<PropertySave> properties)
        {
            if (sourceClassType.StartsWith("FlatRedBall.Math.Collision.AlwaysCollidingListCollisionRelationship"))
            {
                FlatRedBall.Math.Collision.CollisionManager.Self.Relationships.Add((CollisionRelationship)instance);
            }
            else if (sourceClassType.StartsWith("FlatRedBall.Math.Collision.DelegateListVsSingleRelationship"))
            {
                FlatRedBall.Math.Collision.CollisionManager.Self.Relationships.Add((CollisionRelationship)instance);
            }
        }

        private static object[] GetParmsForType(string sourceClassType, object container, List<PropertySave> properties)
        {
            if(sourceClassType.StartsWith("FlatRedBall.Math.Collision.AlwaysCollidingListCollisionRelationship"))
            {
                return new object[] { 
                    GlueDynamicManager.Self.GetProperty(container, (string)properties.First(item => item.Name == "FirstCollisionName").Value) 
                };
            }else if(
                sourceClassType.StartsWith("FlatRedBall.Math.Collision.DelegateListVsSingleRelationship")
                ||
                sourceClassType.StartsWith("FlatRedBall.Math.Collision.CollidableListVsTileShapeCollectionRelationship")
                ||
                sourceClassType.StartsWith("FlatRedBall.Math.Collision.ListVsListRelationship")
            )
            {
                return new object[]
                {
                    GlueDynamicManager.Self.GetProperty(container, (string)properties.First(item => item.Name == "FirstCollisionName").Value),
                    GlueDynamicManager.Self.GetProperty(container, (string)properties.First(item => item.Name == "SecondCollisionName").Value)
                };
            }
            else
            {
                throw new NotImplementedException();
            }

            return new object[] { };
        }

        internal static object InstantiateTypeWith1Generic(Type type, Type genType, object[] args)
        {
            var createType = type.MakeGenericType(genType);

            return Activator.CreateInstance(createType, args);
        }

        internal static object InstantiateTypeWith2Generic(Type type, Type genType1, Type genType2, object[] args)
        {
            var createType = type.MakeGenericType(genType1, genType2);

            return Activator.CreateInstance(createType, args);
        }

        internal static object InstantiateEntity(string typeName)
        {
            return InstantiateEntity(GetType(typeName));
        }

        internal static object InstantiateEntity(Type type)
        {
            var instance = Activator.CreateInstance(type);

            return instance;
        }

        internal static Type GetType(string typeName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !IgnoreAssemblies.Any(ignoreItem => assembly.FullName.StartsWith(ignoreItem))).SelectMany(assembly => assembly.DefinedTypes.Where(subType => subType.FullName.EndsWith(typeName))).FirstOrDefault();
        }

        internal static Type GetType(string typeName, bool includeFRB)
        {
            if (includeFRB)
                return AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !IgnoreAssemblies.Where(ia => !FRBAssemblies.Any(fa => ia == fa)).Any(ignoreItem => assembly.FullName.StartsWith(ignoreItem))).SelectMany(assembly => assembly.DefinedTypes.Where(subType => subType.FullName.EndsWith(typeName))).FirstOrDefault();
            else
                return GetType(typeName);
        }

        internal static void AddItemToList(object list, object value)
        {
            list.GetType().GetMethod("Add").Invoke(list, new object[] { value });
        }
    }
}
