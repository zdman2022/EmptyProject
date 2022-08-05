using EmptyProject.GlueDynamicManager.DynamicInstances.Containers;
using EmptyProject.GlueDynamicManager.States;
using FlatRedBall;
using FlatRedBall.Math;
using GlueControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmptyProject.GlueDynamicManager.DynamicInstances
{
    internal class DynamicScreen : FlatRedBall.Screens.Screen
    {
        public static string CurrentScreen { get; internal set; }


        private List<PositionedListContainer> _positionedObjectLists = new List<PositionedListContainer>();
        private List<DynamicEntityContainer> _instancedObjects = new List<DynamicEntityContainer>();
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


            FlatRedBall.Math.Geometry.ShapeManager.SuppressAddingOnVisibilityTrue = oldShapeManagerSuppressAdd;
        }

        private void DynamicInitialize()
        {
            _currentScreenState = GlueDynamicManager.Self.GetDynamicScreenState(CurrentScreen);

            if (_currentScreenState == null)
                throw new Exception("Unable to get dynamic screen state");

            for(var i = _currentScreenState.ScreenSave.NamedObjects.Count - 1; i > -1; i--)
            {
                InitializeNamedObject(_currentScreenState.ScreenSave.NamedObjects[i], _currentScreenState);
            }
        }

        private void InitializeNamedObject(NamedObjectSave item, DynamicScreenState _currentScreenState)
        {
            if(item.SourceClassType == "FlatRedBall.Math.PositionedObjectList<T>")
            {
                if(GlueDynamicManager.Self.ContainsEntity(item.SourceClassGenericType))
                {
                    if(GlueDynamicManager.Self.EntityIsDynamic(item.SourceClassGenericType))
                    {
                        var container = new PositionedListContainer
                        {
                            Value = new PositionedObjectList<DynamicEntity>
                            {
                                Name = item.InstanceName
                            },
                            AddToManagers = true,
                            Name = item.InstanceName
                        };
                        _positionedObjectLists.Add(container);

                        for(var cObjIndex = item.ContainedObjects.Count - 1; cObjIndex > -1; cObjIndex--)
                        {
                            var cObj = item.ContainedObjects[cObjIndex];
                            var entityContainer = new DynamicEntityContainer
                            {
                                Name = cObj.InstanceName,
                                AddToManagers = false,
                                Value = new DynamicEntity(GlueDynamicManager.Self.GetDynamicEntityState(cObj.SourceClassType)),
                                InstructionSaves = cObj.InstructionSaves
                            };
                            _instancedObjects.Add(entityContainer);
                            container.Value.Add(entityContainer.Value);
                        }

                        if(_currentScreenState.BaseScreenSave != null)
                        {
                            //Find matching entity
                            for(var findIndex = _currentScreenState.BaseScreenSave.NamedObjects.Count - 1; findIndex > -1; findIndex--)
                            {
                                if (_currentScreenState.BaseScreenSave.NamedObjects[findIndex].InstanceName == item.InstanceName)
                                {
                                    var bno = _currentScreenState.BaseScreenSave.NamedObjects[findIndex];
                                    for (var cObjIndex = bno.ContainedObjects.Count - 1; cObjIndex > -1; cObjIndex--)
                                    {
                                        var cObj = bno.ContainedObjects[cObjIndex];
                                        var entityContainer = new DynamicEntityContainer
                                        {
                                            Name = cObj.InstanceName,
                                            AddToManagers = false,
                                            Value = new DynamicEntity(GlueDynamicManager.Self.GetDynamicEntityState(cObj.SourceClassType)),
                                            InstructionSaves = cObj.InstructionSaves
                                        };
                                        _instancedObjects.Add(entityContainer);
                                        container.Value.Add(entityContainer.Value);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        public override void AddToManagers()
        {
            for (var i = _instancedObjects.Count - 1; i > -1; i--)
            {
                _instancedObjects[i].Value.AddToManagers(mLayer);
            }

            base.AddToManagers();
            CameraSetup.ResetCamera(SpriteManager.Camera);
        }
        public override void Activity(bool firstTimeCalled)
        {
            if (!IsPaused)
            {
                for(var polIndex = _positionedObjectLists.Count - 1; polIndex > -1; polIndex--)
                {
                    var list = _positionedObjectLists[polIndex];
                    for(var i = list.Value.Count - 1; i > -1; i--)
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