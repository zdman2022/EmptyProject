using GlueDynamicManager.Converters;
using GlueDynamicManager.DynamicInstances.Containers;
using GlueDynamicManager.States;
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
using GlueDynamicManager.GlueHelpers;

namespace GlueDynamicManager.DynamicInstances
{
    internal class DynamicScreen : FlatRedBall.Screens.Screen
    {
        public delegate void InitializeDelegate(object caller, bool addToManagers);
        public event InitializeDelegate InitializeEvent;
        public delegate void ActivityDelegate(object caller);
        public event ActivityDelegate ActivityEvent;
        public delegate void ActivityEditModeDelegate(object caller);
        public event ActivityEditModeDelegate ActivityEditModeEvent;
        public delegate void DestroyDelegate(object caller);
        public event DestroyDelegate DestroyEvent;

        public static string CurrentScreen { get; internal set; }


        private readonly List<PositionedListContainer> _positionedObjectLists = new List<PositionedListContainer>();

        // Do we want a second list for entities?
        private readonly List<DynamicEntityContainer> _instancedEntities = new List<DynamicEntityContainer>();
        private readonly List<ObjectContainer> _instancedObjects = new List<ObjectContainer>();
        private DynamicScreenState _currentScreenState;

        public DynamicScreen() : base("DynamicScreen")
        {
        }
        public override void Initialize(bool addToManagers)
        {
            DynamicInitialize();

            PostInitialize();
            base.Initialize(addToManagers);

            InitializeEvent?.Invoke(this, addToManagers);

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
                var itemContainer = NamedObjectSaveHelper.GetContainerFor(nos, _currentScreenState.ScreenSave);
                NamedObjectSaveHelper.InitializeNamedObject(nos, itemContainer, _currentScreenState.ScreenSave, PropertyFinder, out var positionedObjectLists, out var instancedObjects, out var instancedEntities);

                _positionedObjectLists.AddRange(positionedObjectLists);
                _instancedObjects.AddRange(instancedObjects);
                _instancedEntities.AddRange(instancedEntities);
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
                InstanceAddToManager.AddToManager(_instancedObjects[i], _instancedObjects, layer);
            }

            base.AddToManagers();

            //Might need to do this from json file
            //CameraSetup.ResetCamera(SpriteManager.Camera);
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
                        var instance = list.Value[i];
                        instance.GetType().GetMethod("Activity").Invoke(instance, new object[] { });
                    }
                }
            }

            base.Activity(firstTimeCalled);

            ActivityEvent?.Invoke(this);
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

                ActivityEditModeEvent?.Invoke(this);
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
                    var instance = list.Value[i];
                    instance.GetType().GetMethod("Destroy").Invoke(instance, new object[] { });
                }
                list.Value.MakeTwoWay();
            }

            FlatRedBall.Math.Collision.CollisionManager.Self.Relationships.Clear();

            DestroyEvent?.Invoke(this);
        }
    }
}