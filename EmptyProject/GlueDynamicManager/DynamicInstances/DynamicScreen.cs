using EmptyProject.GlueDynamicManager.Converters;
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

                if (instance.InstructionSaves != null)
                    foreach (var instruction in instance.InstructionSaves)
                    {
                        var convertedValue = ValueConverter.ConvertValue(instruction, this._currentScreenState.ScreenSave);
                        convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, instance.ObjectType);
                        base.ApplyVariable(instruction.Member, convertedValue, instance.Value);
                    }
            }

            for (int i = 0; i < _instancedEntities.Count; i++)
            {
                var instance = _instancedEntities[i];

                if (instance.InstructionSaves != null)
                    foreach (var instruction in instance.InstructionSaves)
                    {
                        var convertedValue = ValueConverter.ConvertValue(instruction, this._currentScreenState.ScreenSave);
                        convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, typeof(DynamicEntity).Name);
                        base.ApplyVariable(instruction.Member, convertedValue, instance.Value);
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
                    else if (item.SourceClassType.StartsWith("FlatRedBall.Math.Collision.ListVsListRelationship<"))
                        return 100;

                    return 50;
                })
                .ToArray();

            foreach (var nos in namedObjects)
            {
                var itemContainer = GetContainerFor(nos, _currentScreenState.ScreenSave);
                InitializeNamedObject(nos, itemContainer, _currentScreenState);
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

        private void InitializeNamedObject(NamedObjectSave nos, NamedObjectSave nosList, DynamicScreenState _currentScreenState)
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
                            AddToManagers = true,
                            Name = nos.InstanceName
                        };
                        _positionedObjectLists.Add(container);
                    }
                }
            }
            else if (nos.SourceClassType.StartsWith("FlatRedBall.Math.Collision.ListVsListRelationship<"))
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

                }
                // Entity vs. List
                else if (value1.GetValue() is PositionedObject && value2.GetValue().GetType().GetGenericTypeDefinition() == typeof(PositionedObjectList<>))
                {

                }
                // List vs. Entity
                else if (value1.GetValue().GetType().GetGenericTypeDefinition() == typeof(PositionedObjectList<>) && value2.GetValue() is PositionedObject)
                {

                }
                // List vs. List
                else if (value1.GetValue().GetType().GetGenericTypeDefinition() == typeof(PositionedObjectList<>) && value2.GetValue().GetType().GetGenericTypeDefinition() == typeof(PositionedObjectList<>))
                {
                    var listVsListMethod = methods.Where(item => item.ReturnType.GetGenericTypeDefinition() == typeof(ListVsListRelationship<,>)).First();

                    var genericMethod = listVsListMethod.MakeGenericMethod(value1.GetValue().GetType().GetGenericArguments()[0], value2.GetValue().GetType().GetGenericArguments()[0]);

                    var returnValue = genericMethod.Invoke(CollisionManager.Self, System.Reflection.BindingFlags.Default, null, new object[] { value1.GetValue(), value2.GetValue() }, CultureInfo.InvariantCulture);

                    var entityContainer = new ObjectContainer
                    {
                        Name = nos.InstanceName,
                        AddToManagers = false,
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

                }
                // List vs. ShapeCollection
                else if (value1.GetValue().GetType().GetGenericTypeDefinition() == typeof(PositionedObjectList<>) && value2.GetValue() is ShapeCollection)
                {

                }

                //var collideList = CollisionManager.Self.CreateRelationship()
            }
            else if (nos.SourceType == SourceType.Entity)
            {
                var entityContainer = new DynamicEntityContainer
                {
                    Name = nos.InstanceName,
                    AddToManagers = false,
                    Value = new DynamicEntity(GlueDynamicManager.Self.GetDynamicEntityState(nos.SourceClassType)),
                    InstructionSaves = GetInstructionsRecursively(nos, _currentScreenState.ScreenSave)
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
                    ObjectType = nos.SourceClassType,
                    Name = nos.InstanceName,
                    AddToManagers = nos.AddToManagers,
                    InstructionSaves = GetInstructionsRecursively(nos, _currentScreenState.ScreenSave)
                };
                objectContainer.Value = InstanceInstantiator.Instantiate(nos.SourceClassType);

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
                InstanceAddToManager.AddToManager(_instancedObjects[i].Value);
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