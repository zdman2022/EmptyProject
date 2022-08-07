﻿using EmptyProject.GlueDynamicManager.Converters;
using EmptyProject.GlueDynamicManager.DynamicInstances.Containers;
using EmptyProject.GlueDynamicManager.States;
using FlatRedBall;
using FlatRedBall.Content.Instructions;
using FlatRedBall.Math;
using FlatRedBall.Math.Collision;
using FlatRedBall.Math.Geometry;
using GlueControl.Managers;
using GlueControl.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmptyProject.GlueDynamicManager.DynamicInstances
{
    internal class DynamicScreen : FlatRedBall.Screens.Screen
    {
        public static string CurrentScreen { get; internal set; }


        private List<PositionedListContainer> _positionedObjectLists = new List<PositionedListContainer>();

        // Do we want a second list for entities?
        private List<DynamicEntityContainer> _instancedEntities = new List<DynamicEntityContainer>();
        private List<ObjectContainer> _instancedObjects = new List<ObjectContainer>();
        private DynamicScreenState _currentScreenState;

        public DynamicScreen() : base("DynamicScreen")
        {
        }
        public override void Initialize(bool addToManagers)
        {
            DynamicInitialize();

            PostInitialize();
            base.Initialize(addToManagers);
            if (addToManagers)
            {
                AddToManagers();
            }
        }

        private void PostInitialize()
        {
            bool oldShapeManagerSuppressAdd = FlatRedBall.Math.Geometry.ShapeManager.SuppressAddingOnVisibilityTrue;
            FlatRedBall.Math.Geometry.ShapeManager.SuppressAddingOnVisibilityTrue = true;

            for (int i = 0; i < _instancedObjects.Count; i++)
            {
                var instance = _instancedObjects[i];

                if (instance.CombinedInstructionSaves != null)
                {
                    foreach (var instruction in instance.CombinedInstructionSaves)
                    {
                        var convertedValue = ValueConverter.ConvertValue(instruction, this._currentScreenState.ScreenSave);
                        convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, instance.ObjectType);

                        // handle special cases here:
                        var handledByAssigner = InstanceVariableAssigner.TryAssignVariable(instruction.Member, convertedValue, instance.Value);

                        if(!handledByAssigner)
                        {
                            base.ApplyVariable(instruction.Member, convertedValue, instance.Value);
                        }
                    }
                }
            }

            for (int i = 0; i < _instancedEntities.Count; i++)
            {
                var instance = _instancedEntities[i];

                if (instance.CombinedInstructionSaves != null)
                    foreach (var instruction in instance.CombinedInstructionSaves)
                    {
                        var convertedValue = ValueConverter.ConvertValue(instruction, this._currentScreenState.ScreenSave);
                        convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, typeof(DynamicEntity).Name);
                        instance.Value.SetVariable(instruction.Member, convertedValue);
                    }
            }

            FlatRedBall.Math.Geometry.ShapeManager.SuppressAddingOnVisibilityTrue = oldShapeManagerSuppressAdd;
        }

        private void DynamicInitialize()
        {
            _currentScreenState = GlueDynamicManager.Self.GetDynamicScreenState(CurrentScreen);

            if (_currentScreenState == null)
                throw new Exception("Unable to get dynamic screen state");

            var namedObjects = _currentScreenState.ScreenSave.AllNamedObjects
                .OrderBy(item =>
                {
                    if (item.SourceClassType == "FlatRedBall.Math.PositionedObjectList<T>")
                        return 1;
                    else if (item.SourceClassType?.StartsWith("FlatRedBall.Math.Collision.ListVsListRelationship<") == true)
                        return 100;

                    return 50;
                })
                .ToArray();

            foreach (var nos in namedObjects)
            {
                var itemContainer = GetContainerFor(nos, _currentScreenState.ScreenSave);
                InitializeNamedObject(nos, itemContainer, _currentScreenState.ScreenSave);
            }
        }

        private NamedObjectSave GetContainerFor(NamedObjectSave nos, ScreenSave screenSave)
        {
            foreach (var candidate in screenSave.NamedObjects)
            {
                if (candidate.ContainedObjects.Any(item => item.InstanceName == nos.InstanceName))
                {
                    return candidate;
                }
            }

            if (screenSave.BaseElement != null)
            {
                var baseScreen = ObjectFinder.Self.GetBaseElement(screenSave);

                if (baseScreen is ScreenSave baseScreenSave)
                {
                    return GetContainerFor(nos, baseScreenSave);
                }
            }

            return null;
        }

        private void InitializeNamedObject(NamedObjectSave nos, NamedObjectSave nosList, ScreenSave glueElement)
        {
            if (nos.SourceClassType == "FlatRedBall.Math.PositionedObjectList<T>")
            {
                if (GlueDynamicManager.Self.ContainsEntity(nos.SourceClassGenericType))
                {
                    if (GlueDynamicManager.Self.EntityIsDynamic(nos.SourceClassGenericType))
                    {
                        var container = new PositionedListContainer
                        {
                            Value = new PositionedObjectList<DynamicEntity>
                            {
                                Name = nos.InstanceName
                            },
                            NamedObjectSave = nos
                        };
                        _positionedObjectLists.Add(container);
                    }
                }
            }
            else if (nos.SourceClassType?.StartsWith("FlatRedBall.Math.Collision.ListVsListRelationship<") == true)
            {
                var name1 = (string)nos.Properties.Where(item => item.Name == "FirstCollisionName").Select(item => item.Value).First();
                var name2 = (string)nos.Properties.Where(item => item.Name == "SecondCollisionName").Select(item => item.Value).First();
                var item1 = PropertyFinder(name1);
                var item2 = PropertyFinder(name2);

                var value1 = item1 as IInstanceContainer;
                var value2 = item2 as IInstanceContainer;

                var methods = typeof(CollisionManager).GetMethods().Where(item => item.Name == "CreateRelationship").ToList();

                // Entity vs. Entity
                if (value1.GetValue() is PositionedObject && value2.GetValue() is PositionedObject)
                {
                    throw new NotImplementedException();
                }
                // Entity vs. List
                else if (value1.GetValue() is PositionedObject && value2.GetValue().GetType().GetGenericTypeDefinition() == typeof(PositionedObjectList<>))
                {
                    throw new NotImplementedException();
                }
                // List vs. Entity
                else if (value1.GetValue().GetType().GetGenericTypeDefinition() == typeof(PositionedObjectList<>) && value2.GetValue() is PositionedObject)
                {
                    throw new NotImplementedException();
                }
                // List vs. List
                else if (value1.GetValue().GetType().GetGenericTypeDefinition() == typeof(PositionedObjectList<>) && value2.GetValue().GetType().GetGenericTypeDefinition() == typeof(PositionedObjectList<>))
                {
                    var listVsListMethod = methods.Where(item => item.ReturnType.GetGenericTypeDefinition() == typeof(ListVsListRelationship<,>)).First();

                    var genericMethod = listVsListMethod.MakeGenericMethod(value1.GetValue().GetType().GetGenericArguments()[0], value2.GetValue().GetType().GetGenericArguments()[0]);

                    var returnValue = genericMethod.Invoke(CollisionManager.Self, System.Reflection.BindingFlags.Default, null, new object[] { value1.GetValue(), value2.GetValue() }, CultureInfo.InvariantCulture);

                    var entityContainer = new ObjectContainer
                    {
                        NamedObjectSave = nos,
                        Value = returnValue
                    };
                    _instancedObjects.Add(entityContainer);

                    var prop = returnValue.GetType().GetProperty("CollisionLimit", BindingFlags.Public | BindingFlags.Instance);
                    prop.SetValue(returnValue, FlatRedBall.Math.Collision.CollisionLimit.All);

                    prop = returnValue.GetType().GetProperty("ListVsListLoopingMode", BindingFlags.Public | BindingFlags.Instance);
                    prop.SetValue(returnValue, FlatRedBall.Math.Collision.ListVsListLoopingMode.PreventDoubleChecksPerFrame);
                }
                // Entity vs ShapeCollection
                else if (value1.GetValue() is PositionedObject && value2.GetValue() is ShapeCollection)
                {
                    throw new NotImplementedException();
                }
                // List vs. ShapeCollection
                else if (value1.GetValue().GetType().GetGenericTypeDefinition() == typeof(PositionedObjectList<>) && value2.GetValue() is ShapeCollection)
                {
                    throw new NotImplementedException();
                }

                //var collideList = CollisionManager.Self.CreateRelationship()
            }
            else if (nos.SourceType == SourceType.Entity)
            {
                var entityContainer = new DynamicEntityContainer
                {
                    NamedObjectSave = nos,
                    Value = new DynamicEntity(GlueDynamicManager.Self.GetDynamicEntityState(nos.SourceClassType)),
                    CombinedInstructionSaves = GetInstructionsRecursively(nos, glueElement)
                };
                _instancedEntities.Add(entityContainer);

                if (nosList != null)
                {
                    var container = _positionedObjectLists.Find(item => item.Name == nosList.InstanceName);
                    if (container != null)
                    {
                        container.Value.Add(entityContainer.Value);
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

                if(nos.SourceType == SourceType.File)
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

                    if(glueElement is ScreenSave == false)
                    {
                        
                        // we need to clone the file so each entity has its own copy
                        //file = file.Clone();
                    }

                    var isEntireFile = nos.SourceName?.StartsWith("Entire File") == true;
                    if(isEntireFile)
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
                    objectContainer.Value = InstanceInstantiator.Instantiate(nos.SourceClassType);
                }

                _instancedObjects.Add(objectContainer);

            }
        }

        private object PropertyFinder(string name1)
        {
            object foundItem;

            foundItem = _positionedObjectLists.Where(item => item.Name == name1).FirstOrDefault();

            if (foundItem != null)
                return foundItem;

            foundItem = _instancedObjects.Where(item => item.Name == name1).FirstOrDefault();

            if (foundItem != null)
                return foundItem;

            return null;
        }

        private List<InstructionSave> GetInstructionsRecursively(NamedObjectSave nos, GlueElement glueElement, List<InstructionSave> list = null)
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

        public override void AddToManagers()
        {
            for (int i = 0; i < _instancedEntities.Count; i++)
            {
                _instancedEntities[i].Value.AddToManagers(mLayer);
            }

            for (var i = 0; i < _instancedObjects.Count; i++)
            {
                // todo: need to support layers
                FlatRedBall.Graphics.Layer layer = null;
                InstanceAddToManager.AddToManager(_instancedObjects[i].Value, layer);
            }

            base.AddToManagers();
            CameraSetup.ResetCamera(SpriteManager.Camera);
        }
        public override void Activity(bool firstTimeCalled)
        {
            if (!IsPaused)
            {
                for (var polIndex = _positionedObjectLists.Count - 1; polIndex > -1; polIndex--)
                {
                    var list = _positionedObjectLists[polIndex];
                    for (var i = list.Value.Count - 1; i > -1; i--)
                    {
                        list.Value[i].Activity();
                    }
                }
            }

            base.Activity(firstTimeCalled);
        }
        public override void ActivityEditMode()
        {
            if (FlatRedBall.Screens.ScreenManager.IsInEditMode)
            {
                foreach (var item in FlatRedBall.SpriteManager.ManagedPositionedObjects)
                {
                    if (item is FlatRedBall.Entities.IEntity entity)
                    {
                        entity.ActivityEditMode();
                    }
                }

                base.ActivityEditMode();
            }
        }
        public override void Destroy()
        {
            base.Destroy();

            for (var polIndex = _positionedObjectLists.Count; polIndex > -1; polIndex--)
            {
                var list = _positionedObjectLists[polIndex];
                list.Value.MakeOneWay();
                for (var i = list.Value.Count - 1; i > -1; i--)
                {
                    list.Value[i].Destroy();
                }
                list.Value.MakeTwoWay();
            }

            FlatRedBall.Math.Collision.CollisionManager.Self.Relationships.Clear();
        }
    }
}