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
using System.Collections;

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

        public static string CurrentScreenGlue { get; internal set; }
        public string TypeName { get; }


        // Do we want a second list for entities?
        private readonly List<ObjectContainer> _instancedObjects = new List<ObjectContainer>();
        private DynamicScreenState _currentScreenState;

        public DynamicScreen() : base("DynamicScreen")
        {
            TypeName = CurrentScreenGlue;
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

                if(instance.Value is DynamicEntity dynamicEntity)
                {
                    if (instance.CombinedInstructionSaves != null)
                        foreach (var instruction in instance.CombinedInstructionSaves)
                        {
                            var convertedValue = ValueConverter.ConvertValue(instruction, this._currentScreenState.ScreenSave);
                            convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, typeof(DynamicEntity).Name);
                            dynamicEntity.SetVariable(instruction.Member, convertedValue);
                        }
                }
                else
                {
                    if (instance.CombinedInstructionSaves != null)
                    {
                        foreach (var instruction in instance.CombinedInstructionSaves)
                        {
                            var convertedValue = ValueConverter.ConvertValue(instruction, this._currentScreenState.ScreenSave);
                            convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, instance.ObjectType);

                            // handle special cases here:
                            var handledByAssigner = InstanceVariableAssigner.TryAssignVariable(instruction.Member, convertedValue, instance.Value);

                            if (!handledByAssigner)
                            {
                                base.ApplyVariable(instruction.Member, convertedValue, instance.Value);
                            }
                        }
                    }
                }
            }

            FlatRedBall.Math.Geometry.ShapeManager.SuppressAddingOnVisibilityTrue = oldShapeManagerSuppressAdd;
        }

        private void DynamicInitialize()
        {
            _currentScreenState = GlueDynamicManager.Self.GetDynamicScreenState(CurrentScreenGlue);

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
                NamedObjectSaveHelper.InitializeNamedObject(this, nos, itemContainer, _currentScreenState.ScreenSave, PropertyFinder, out var instancedObjects);

                _instancedObjects.AddRange(instancedObjects);
            }
        }

        

        

        public object PropertyFinder(string name1)
        {
            var foundItem = _instancedObjects.Where(item => item.Name == name1).FirstOrDefault();

            if (foundItem != null)
                return foundItem.Value;

            return null;
        }

        

        public override void AddToManagers()
        {
            for (var i = 0; i < _instancedObjects.Count; i++)
            {
                var instance = _instancedObjects[i];

                if(instance.Value is DynamicEntity dynamicEntity)
                {
                    dynamicEntity.AddToManagers(mLayer);
                }
                else
                {
                    // todo: need to support layers
                    FlatRedBall.Graphics.Layer layer = null;
                    InstanceAddToManager.AddToManager(_instancedObjects[i], _instancedObjects, layer);
                }
                
            }

            base.AddToManagers();

            //Might need to do this from json file
            //CameraSetup.ResetCamera(SpriteManager.Camera);
        }
        public override void Activity(bool firstTimeCalled)
        {
            if (!IsPaused)
            {
                for (int i = _instancedObjects.Count - 1; i > -1; i--)
                {
                    var instance = _instancedObjects[i].Value;
                    if (instance is IEnumerable enumerable)
                    {
                        foreach(var item in enumerable)
                        {
                            item.GetType().GetMethod("Activity").Invoke(item, new object[] { });
                        }
                    }
                    else if(instance is DynamicEntity asDynamicEntity)
                    {
                        asDynamicEntity.Activity();
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

            for (int i = _instancedObjects.Count - 1; i > -1; i--)
            {
                InstanceDestroy.Destroy(_instancedObjects[i]);
            }

            FlatRedBall.Math.Collision.CollisionManager.Self.Relationships.Clear();

            DestroyEvent?.Invoke(this);
        }
    }
}