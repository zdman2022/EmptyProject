using FlatRedBall;
using FlatRedBall.Graphics;
using FlatRedBall.Math.Collision;
using FlatRedBall.Math.Geometry;
using FlatRedBall.TileGraphics;
using GlueControl;
using GlueControl.Models;
using GlueDynamicManager.Converters;
using GlueDynamicManager.DynamicInstances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        internal static object Instantiate(NamedObjectSave nos, object container)
        {
            string sourceClassType = nos.SourceClassType;
            List<PropertySave> properties = nos.Properties;

            if (sourceClassType == typeof(AxisAlignedCube).FullName)
            {
                return new AxisAlignedCube()
                {
                    Name = nos.InstanceName,
                    CreationSource = "Dynamic"
                };
            }
            if (sourceClassType == typeof(AxisAlignedRectangle).FullName)
            {
                return new AxisAlignedRectangle()
                {
                    Name = nos.InstanceName,
                    CreationSource = "Dynamic"
                };
            }
            else if(sourceClassType == typeof(Circle).FullName)
            {
                return new Circle()
                {
                    Name = nos.InstanceName,
                    CreationSource = "Dynamic"
                };
            }
            else if (sourceClassType == typeof(Line).FullName)
            {
                return new Line()
                {
                    Name = nos.InstanceName,
                    CreationSource = "Dynamic"
                };
            }
            else if (sourceClassType == typeof(Polygon).FullName)
            {
                return new Polygon()
                {
                    Name = nos.InstanceName,
                    CreationSource = "Dynamic"
                };
            }
            else if (sourceClassType == typeof(Sphere).FullName)
            {
                return new Sphere()
                {
                    Name = nos.InstanceName,
                    CreationSource = "Dynamic"
                };
            }
            else if (sourceClassType == typeof(Sprite).FullName)
            {
                return new Sprite()
                {
                    Name = nos.InstanceName,
                    CreationSource = "Dynamic"
                };
            }
            else if (sourceClassType == typeof(Text).FullName)
            {
                return new Text()
                {
                    Name = nos.InstanceName,
                    CreationSource = "Dynamic"
                };
            }
            else if(sourceClassType == typeof(FlatRedBall.TileCollisions.TileShapeCollection).FullName)
            {
                return new FlatRedBall.TileCollisions.TileShapeCollection()
                {
                    Name = nos.InstanceName
                };
            }
            else if (GenericSingleTypeRegEx.IsMatch(sourceClassType))
            {
                var match = GenericSingleTypeRegEx.Match(sourceClassType);
                var typeName = match.Groups[1].Value.Trim();

                var genericTypeNameGlue = nos.SourceClassGenericType;

                string genericTypeGame = null;
                if (string.IsNullOrEmpty(genericTypeNameGlue))
                {
                    // collision relationships have a special approach here where the type is 
                    // using the C# type, we just have to prefix the game namespace:
                    genericTypeGame = CommandReceiver.TopNamespace + "." + match.Groups[2].Value.Trim();
                }
                else
                {
                    genericTypeGame = CommandReceiver.GlueToGameElementName(genericTypeNameGlue);
                }

                Type genType = GetTypeCheckForDynamic(genericTypeGame);


                var parmList = GetParmsForType(sourceClassType, container, properties);

                var instance = InstantiateTypeWith1Generic(GetType(typeName + "`1", true), genType, parmList);

                TypeHandler.SetPropValueIfExists(instance, "Name", nos.InstanceName);
                TypeHandler.SetPropValueIfExists(instance, "CreationSource", "Dynamic");

                ApplyPropertiesToInstance(sourceClassType, instance, container, properties);

                return instance;
                
            }
            else if (GenericDoubleTypeRegEx.IsMatch(sourceClassType))
            {
                var match = GenericDoubleTypeRegEx.Match(sourceClassType);
                var typeName = match.Groups[1].Value.Trim();
                var genTypeName1 = CommandReceiver.TopNamespace + "." + match.Groups[2].Value.Trim();
                var genTypeName2 = CommandReceiver.TopNamespace + "." + match.Groups[3].Value.Trim();

                Type genType1 = GetTypeCheckForDynamic(genTypeName1);
                Type genType2 = GetTypeCheckForDynamic(genTypeName2);

                var parmList = GetParmsForType(sourceClassType, container, properties);

                var instance = InstantiateTypeWith2Generic(GetType(typeName + "`2", true), genType1, genType2, parmList);

                TypeHandler.SetPropValueIfExists(instance, "Name", nos.InstanceName);
                TypeHandler.SetPropValueIfExists(instance, "CreationSource", "Dynamic");

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

        private static Type GetTypeCheckForDynamic(string typeNameGame)
        {
            Type genType;

            var glueName = CommandReceiver.GameElementTypeToGlueElement(typeNameGame);

            if (glueName.StartsWith("Entities\\") && GlueDynamicManager.Self.EntityIsDynamic(glueName))
            {
                genType = typeof(DynamicEntity);
            }
            else
            {
                genType = GetType(typeNameGame, true);
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
            else if (sourceClassType.StartsWith("FlatRedBall.Math.Collision.ListVsListRelationship"))
            {
                var match = GenericDoubleTypeRegEx.Match(sourceClassType);
                var typeName = match.Groups[1].Value.Trim();
                var genTypeName1 = match.Groups[2].Value.Trim();
                var genTypeName2 = match.Groups[3].Value.Trim();

                var genericTypes = instance.GetType().GenericTypeArguments;
                var FirstSubCollisionSelectedItem = properties.Where(item => item.Name == "FirstSubCollisionSelectedItem").Select(item => (string)item.Value).FirstOrDefault();
                var SecondSubCollisionSelectedItem = properties.Where(item => item.Name == "SecondSubCollisionSelectedItem").Select(item => (string)item.Value).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(FirstSubCollisionSelectedItem) && FirstSubCollisionSelectedItem != "<Entire Object>")
                {
                    var d = GetSubCollisionDelegate(genTypeName1, genericTypes[0], FirstSubCollisionSelectedItem);
                    instance.GetType().GetMethods().Where(item => item.Name == "SetFirstSubCollision" && item.GetParameters()[0].ParameterType == d.GetType()).First().Invoke(instance, new object[] { d, FirstSubCollisionSelectedItem});
                }

                if (!string.IsNullOrWhiteSpace(SecondSubCollisionSelectedItem) && SecondSubCollisionSelectedItem != "<Entire Object>")
                {
                    var d = GetSubCollisionDelegate(genTypeName1, genericTypes[1], SecondSubCollisionSelectedItem);
                    instance.GetType().GetMethods().Where(item => item.Name == "SetSecondSubCollision" && item.GetParameters()[0].ParameterType == d.GetType()).First().Invoke(instance, new object[] { d, SecondSubCollisionSelectedItem });
                }
                //    indexer.GetType().

                instance.GetType().GetProperty("CollisionLimit").SetValue(instance, FlatRedBall.Math.Collision.CollisionLimit.All);
                instance.GetType().GetProperty("ListVsListLoopingMode").SetValue(instance, FlatRedBall.Math.Collision.ListVsListLoopingMode.PreventDoubleChecksPerFrame, null);
            }
            else
            {
                
            }
        }

        private static object GetSubCollisionDelegate(string entityType, Type instanceType, string subCollision)
        {
            var entityState = GlueDynamicManager.Self.GetEntityState(entityType);

            var no = entityState.EntitySave.NamedObjects.Where(item => item.InstanceName == subCollision).FirstOrDefault();

            if (no == null)
                throw new Exception("Sub collision not found");

            var newType = typeof(Func<,>);
            var method = typeof(InstanceInstantiator).GetMethod("GetTypedProperty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            if (no.InstanceType == "FlatRedBall.Math.Geometry.Circle")
            {
                newType = newType.MakeGenericType(instanceType, typeof(Circle));
                method = method.MakeGenericMethod(instanceType, typeof(Circle));
                return Delegate.CreateDelegate(newType, null, (MethodInfo)method.Invoke(null, new object[] { subCollision }));
            }else if(no.InstanceType == "FlatRedBall.Math.Geometry.AxisAlignedRectangle")
            {
                newType = newType.MakeGenericType(instanceType, typeof(AxisAlignedRectangle));
                method = method.MakeGenericMethod(instanceType, typeof(AxisAlignedRectangle));
                return Delegate.CreateDelegate(newType, null, (MethodInfo)method.Invoke(null, new object[] { subCollision }));
            }
            else if (no.InstanceType == "FlatRedBall.Math.Geometry.Polygon")
            {
                newType = newType.MakeGenericType(instanceType, typeof(Polygon));
                method = method.MakeGenericMethod(instanceType, typeof(Polygon));
                return Delegate.CreateDelegate(newType, null, (MethodInfo)method.Invoke(null, new object[] { subCollision }));
            }
            else if (no.InstanceType == "FlatRedBall.Math.Geometry.Line")
            {
                newType = newType.MakeGenericType(instanceType, typeof(Line));
                method = method.MakeGenericMethod(instanceType, typeof(Line));
                return Delegate.CreateDelegate(newType, null, (MethodInfo)method.Invoke(null, new object[] { subCollision }));
            }
            //ICollidable
            else
            {
                newType = newType.MakeGenericType(instanceType, typeof(ICollidable));
                method = method.MakeGenericMethod(instanceType, typeof(ICollidable));
                return Delegate.CreateDelegate(newType, null, (MethodInfo)method.Invoke(null, new object[] { subCollision }));
            }
        }

#pragma warning disable IDE0051 // Remove unused private members
        private static MethodInfo GetTypedProperty<T, V>(string propName)
#pragma warning restore IDE0051 // Remove unused private members
        {
            var returnValue = GetTypedPropertyFunc<T, V>(propName);

            return returnValue.GetMethodInfo();
        }

        private static Func<T, V> GetTypedPropertyFunc<T, V>(string propName)
        {
            return (T item) => { return (V)item.GetType().GetMethod("PropertyFinder").Invoke(item, new object[] { propName }); };
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
                //throw new NotImplementedException();
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
            var instance = Activator.CreateInstance(type, new object[] { FlatRedBall.Screens.ScreenManager.CurrentScreen.ContentManagerName, false });

            return instance;
        }

        internal static Type GetType(string typeName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !IgnoreAssemblies.Any(ignoreItem => assembly.FullName.StartsWith(ignoreItem))).SelectMany(assembly => assembly.DefinedTypes.Where(subType => subType.FullName.EndsWith(typeName))).FirstOrDefault();
        }

        internal static Type GetType(string typeNameGame, bool includeFRB)
        {
            if (includeFRB)
                return AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !IgnoreAssemblies.Where(ia => !FRBAssemblies.Any(fa => ia == fa)).Any(ignoreItem => assembly.FullName.StartsWith(ignoreItem))).SelectMany(assembly => assembly.DefinedTypes.Where(subType => subType.FullName.EndsWith(typeNameGame))).FirstOrDefault();
            else
                return GetType(typeNameGame);
        }

        internal static void AddItemToList(object list, object value)
        {
            list.GetType().GetMethod("Add").Invoke(list, new object[] { value });
        }
    }
}
