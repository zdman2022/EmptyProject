using FlatRedBall;
using FlatRedBall.Content.Instructions;
using FlatRedBall.Math;
using FlatRedBall.Math.Collision;
using FlatRedBall.Math.Geometry;
using GlueControl.Managers;
using GlueControl.Models;
using GlueDynamicManager.Converters;
using GlueDynamicManager.DynamicInstances;
using GlueDynamicManager.DynamicInstances.Containers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace GlueDynamicManager.GlueHelpers
{
    internal class NamedObjectSaveHelper
    {
        public static NamedObjectSave GetContainerFor(NamedObjectSave nos, GlueElement element)
        {
            foreach (var candidate in element.NamedObjects)
            {
                if (candidate.ContainedObjects.Any(item => item.InstanceName == nos.InstanceName))
                {
                    return candidate;
                }
            }

            if (element.BaseElement != null)
            {
                var baseScreen = ObjectFinder.Self.GetBaseElement(element);

                if (baseScreen is ScreenSave baseScreenSave)
                {
                    return GetContainerFor(nos, baseScreenSave);
                }
            }

            return null;
        }

        public static void InitializeNamedObject(object noContainer, NamedObjectSave nos, NamedObjectSave nosList, GlueElement glueElement, Func<string, object> propertyFinder, out List<ObjectContainer> instancedObjects)
        {
            instancedObjects = new List<ObjectContainer>();

            if (nos.SourceClassType == "FlatRedBall.Math.PositionedObjectList<T>")
            {
                if (GlueDynamicManager.Self.ContainsEntity(nos.SourceClassGenericType))
                {
                    if (GlueDynamicManager.Self.EntityIsDynamic(nos.SourceClassGenericType))
                    {
                        var container = new ObjectContainer
                        {
                            Value = new PositionedObjectList<DynamicEntity>
                            {
                                Name = nos.InstanceName
                            },
                            NamedObjectSave = nos
                        };
                        instancedObjects.Add(container);
                    }
                }
            }
            //else if (nos.SourceClassType?.StartsWith("FlatRedBall.Math.Collision.ListVsListRelationship<") == true)
            //{
            //    var name1 = (string)nos.Properties.Where(item => item.Name == "FirstCollisionName").Select(item => item.Value).First();
            //    var name2 = (string)nos.Properties.Where(item => item.Name == "SecondCollisionName").Select(item => item.Value).First();
            //    var item1 = propertyFinder(name1);
            //    var item2 = propertyFinder(name2);

            //    var value1 = item1 as IInstanceContainer;
            //    var value2 = item2 as IInstanceContainer;

            //    var methods = typeof(CollisionManager).GetMethods().Where(item => item.Name == "CreateRelationship").ToList();

            //    // Entity vs. Entity
            //    if (value1.GetValue() is PositionedObject && value2.GetValue() is PositionedObject)
            //    {
            //        var listVsListMethod = methods.Where(item => item.ReturnType.GetGenericTypeDefinition() == typeof(ListVsListRelationship<,>)).First();

            //        var genericMethod = listVsListMethod.MakeGenericMethod(value1.GetValue().GetType().GetGenericArguments()[0], value2.GetValue().GetType().GetGenericArguments()[0]);

            //        var returnValue = genericMethod.Invoke(CollisionManager.Self, System.Reflection.BindingFlags.Default, null, new object[] { value1.GetValue(), value2.GetValue() }, CultureInfo.InvariantCulture);

            //        var entityContainer = new ObjectContainer
            //        {
            //            NamedObjectSave = nos,
            //            Value = returnValue
            //        };
            //        instancedObjects.Add(entityContainer);

            //        var prop = returnValue.GetType().GetProperty("CollisionLimit", BindingFlags.Public | BindingFlags.Instance);
            //        prop.SetValue(returnValue, FlatRedBall.Math.Collision.CollisionLimit.All);

            //        prop = returnValue.GetType().GetProperty("ListVsListLoopingMode", BindingFlags.Public | BindingFlags.Instance);
            //        prop.SetValue(returnValue, FlatRedBall.Math.Collision.ListVsListLoopingMode.PreventDoubleChecksPerFrame);
            //    }
            //    // Entity vs. List
            //    else if (value1.GetValue() is PositionedObject && value2.GetValue().GetType().GetGenericTypeDefinition() == typeof(PositionedObjectList<>))
            //    {
            //        throw new NotImplementedException();
            //    }
            //    // List vs. Entity
            //    else if (value1.GetValue().GetType().GetGenericTypeDefinition() == typeof(PositionedObjectList<>) && value2.GetValue() is PositionedObject)
            //    {
            //        throw new NotImplementedException();
            //    }
            //    // List vs. List
            //    else if (value1.GetValue().GetType().GetGenericTypeDefinition() == typeof(PositionedObjectList<>) && value2.GetValue().GetType().GetGenericTypeDefinition() == typeof(PositionedObjectList<>))
            //    {
            //        var listVsListMethod = methods.Where(item => item.ReturnType.GetGenericTypeDefinition() == typeof(ListVsListRelationship<,>)).First();

            //        var genericMethod = listVsListMethod.MakeGenericMethod(value1.GetValue().GetType().GetGenericArguments()[0], value2.GetValue().GetType().GetGenericArguments()[0]);

            //        var returnValue = genericMethod.Invoke(CollisionManager.Self, System.Reflection.BindingFlags.Default, null, new object[] { value1.GetValue(), value2.GetValue() }, CultureInfo.InvariantCulture);

            //        var entityContainer = new ObjectContainer
            //        {
            //            NamedObjectSave = nos,
            //            Value = returnValue
            //        };
            //        instancedObjects.Add(entityContainer);

            //        var prop = returnValue.GetType().GetProperty("CollisionLimit", BindingFlags.Public | BindingFlags.Instance);
            //        prop.SetValue(returnValue, FlatRedBall.Math.Collision.CollisionLimit.All);

            //        prop = returnValue.GetType().GetProperty("ListVsListLoopingMode", BindingFlags.Public | BindingFlags.Instance);
            //        prop.SetValue(returnValue, FlatRedBall.Math.Collision.ListVsListLoopingMode.PreventDoubleChecksPerFrame);
            //    }
            //    // Entity vs ShapeCollection
            //    else if (value1.GetValue() is PositionedObject && value2.GetValue() is ShapeCollection)
            //    {
            //        throw new NotImplementedException();
            //    }
            //    // List vs. ShapeCollection
            //    else if (value1.GetValue().GetType().GetGenericTypeDefinition() == typeof(PositionedObjectList<>) && value2.GetValue() is ShapeCollection)
            //    {
            //        throw new NotImplementedException();
            //    }

            //    //var collideList = CollisionManager.Self.CreateRelationship()
            //}
            else if (nos.SourceType == SourceType.Entity)
            {
                if (GlueDynamicManager.Self.EntityIsDynamic(nos.SourceClassType))
                {
                    var entityContainer = new ObjectContainer
                    {
                        NamedObjectSave = nos,
                        Value = new DynamicEntity(nos.SourceClassType, GlueDynamicManager.Self.GetEntityState(nos.SourceClassType)),
                        CombinedInstructionSaves = GetInstructionsRecursively(nos, glueElement)
                    };
                    instancedObjects.Add(entityContainer);

                    if (nosList != null)
                    {
                        var container = instancedObjects.Find(item => item.Name == nosList.InstanceName);
                        if (container != null)
                        {
                            InstanceInstantiator.AddItemToList(container.Value, entityContainer.Value);
                        }

                    }
                }
                else
                {
                    var objectContainer = new ObjectContainer
                    {
                        NamedObjectSave = nos,
                        Value = InstanceInstantiator.InstantiateEntity(nos.ClassType),
                        CombinedInstructionSaves = GetInstructionsRecursively(nos, glueElement)
                    };
                    instancedObjects.Add(objectContainer);

                    if (nosList != null)
                    {
                        var container = instancedObjects.Find(item => item.Name == nosList.InstanceName);
                        if (container != null)
                        {
                            InstanceInstantiator.AddItemToList(container.Value, objectContainer.Value);
                        }

                    }
                }
            }
            else
            {
                var objectContainer = new ObjectContainer
                {
                    NamedObjectSave = nos,
                    CombinedInstructionSaves = GetInstructionsRecursively(nos, glueElement)
                };

                if (nos.SourceType == SourceType.File)
                {
                    var rfs = glueElement.GetReferencedFileSaveRecursively(nos.SourceFile);
                    var absoluteRfs = GlueCommands.Self.GetAbsoluteFilePath(rfs);


                    // todo - do we need to cache?
                    // For files like AnimationChains
                    // FRB handles the caching internally.
                    // But what about TMX files? I am not sure
                    // how this works in codegen. For screens it
                    // doesn't matter, but it might for entities?
                    // Revisit this when we start implementing rooms.
                    // todo 2 - need to support global content vs non global content
                    var contentManagerName = FlatRedBallServices.GlobalContentManager;
                    var file = FileLoader.LoadFile(absoluteRfs, contentManagerName);

                    if (glueElement is ScreenSave == false)
                    {

                        // we need to clone the file so each entity has its own copy
                        //file = file.Clone();
                    }

                    var isEntireFile = nos.SourceName?.StartsWith("Entire File") == true;
                    if (isEntireFile)
                    {
                        objectContainer.Value = file;
                    }
                    else
                    {
                        throw new NotImplementedException("need to support pulling specific item out of files");
                    }
                }
                else
                {
                    objectContainer.Value = InstanceInstantiator.Instantiate(nos.SourceClassType, nos.Properties, noContainer);
                }

                instancedObjects.Add(objectContainer);

            }
        }

        public static List<InstructionSave> GetInstructionsRecursively(NamedObjectSave nos, GlueElement glueElement, List<InstructionSave> list = null)
        {
            list = list ?? new List<InstructionSave>();

            if (glueElement.BaseElement != null)
            {
                var baseScreen = ObjectFinder.Self.GetScreenSave(glueElement.BaseElement);

                if (baseScreen != null)
                {
                    GetInstructionsRecursively(nos, baseScreen, list);
                }
            }

            // this could be different than the nos because we may be in a base screen where the references aren't the same
            var itemInContainer = glueElement.AllNamedObjects.FirstOrDefault(item => item.InstanceName == nos.InstanceName);
            if (itemInContainer != null)
            {
                list.AddRange(itemInContainer.InstructionSaves);
            }

            return list;
        }
    }
}
