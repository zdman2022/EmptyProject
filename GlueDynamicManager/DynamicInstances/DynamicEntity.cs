using GlueDynamicManager.Converters;
using GlueDynamicManager.DynamicInstances.Containers;
using GlueDynamicManager.States;
using FlatRedBall;
using FlatRedBall.Entities;
using FlatRedBall.Graphics;
using FlatRedBall.Math.Geometry;
using FlatRedBall.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlueControl;

namespace GlueDynamicManager.DynamicInstances
{
    public class DynamicEntity : PositionedObject, ICollidable, IDestroyable, IEntity
    {
        public delegate void InitializeDelegate(object caller, bool addToManagers);
        public event InitializeDelegate InitializeEvent;
        public delegate void ActivityDelegate(object caller);
        public event ActivityDelegate ActivityEvent;
        public delegate void ActivityEditModeDelegate(object caller);
        public event ActivityEditModeDelegate ActivityEditModeEvent;
        public delegate void DestroyDelegate(object caller);
        public event DestroyDelegate DestroyEvent;

        private FlatRedBall.Math.Geometry.ShapeCollection mGeneratedCollision;
        public FlatRedBall.Math.Geometry.ShapeCollection Collision
        {
            get
            {
                return mGeneratedCollision;
            }
        }

        public string ElementNameGame { get; }

        private EntityState _dynamicEntityState;
        protected FlatRedBall.Graphics.Layer LayerProvidedByContainer = null;
        private List<ObjectContainer> _instancedObjects = new List<ObjectContainer>();

        public DynamicEntity(string nameGlue, EntityState dynamicEntityState)
        {
            ElementNameGame = CommandReceiver.GlueToGameElementName(nameGlue);
            _dynamicEntityState = dynamicEntityState;
            InitializeEntity();
            GlueDynamicManager.Self.AttachEntity(this, true);
            mGeneratedCollision = new FlatRedBall.Math.Geometry.ShapeCollection();
        }

        private void InitializeEntity()
        {
            for (var i = 0; i < _dynamicEntityState.EntitySave.NamedObjects.Count; i++)
            {
                var no = _dynamicEntityState.EntitySave.NamedObjects[i];
                if (GlueDynamicManager.Self.ElementIsDynamic(no.SourceClassType))
                {
                    throw new NotImplementedException();
                }
                else
                {
                    _instancedObjects.Add(new ObjectContainer
                    {
                        NamedObjectSave = no,
                        Value = InstanceInstantiator.Instantiate(no, this),
                        CombinedInstructionSaves = no.InstructionSaves
                    });
                }
            }

            for (var i = 0; i < _instancedObjects.Count; i++)
            {
                var obj = _instancedObjects[i];

                foreach (var instruction in obj.CombinedInstructionSaves)
                {
                    var convertedValue = ValueConverter.ConvertValue(instruction, this._dynamicEntityState.EntitySave);
                    convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, obj.ObjectType);
                    FlatRedBall.Screens.ScreenManager.CurrentScreen.ApplyVariable(instruction.Member, convertedValue, obj.Value);
                }

                if (obj.Value is PositionedObject po)
                {
                    po.CopyAbsoluteToRelative();
                    po.AttachTo(this, false);
                }
            }

            if (InitializeEvent != null)
                InitializeEvent(this, true);
        }

        public void Activity()
        {
            if (ActivityEvent != null)
                ActivityEvent(this);
        }

        public void Destroy()
        {
            for (int i = _instancedObjects.Count - 1; i > -1; i--)
            {
                InstanceDestroy.Destroy(_instancedObjects[i]);
            }

            mGeneratedCollision.RemoveFromManagers(clearThis: false);

            if (DestroyEvent != null)
                DestroyEvent(this);

            this.RemoveSelfFromListsBelongingTo();
        }

        internal void SetVariable(string member, object convertedValue)
        {
            if (_dynamicEntityState.CustomVariablesSave.Where(item => item.SourceObject != null).Any(item => item.Name == member))
            {
                var foundCustomVariable = _dynamicEntityState.CustomVariablesSave.First(item => item.Name == member);
                var foundObject = _instancedObjects.Where(item => item.Name == foundCustomVariable.SourceObject).First();

                ScreenManager.CurrentScreen.ApplyVariable(foundCustomVariable.SourceObjectProperty, convertedValue, foundObject.Value);
            }
            else if (_dynamicEntityState.CustomVariablesSave.Any(item => item.Name == member && item.Properties.Any(item2 => item2.Name == "Type" && _dynamicEntityState.StateCategoryList.Any(item3 => item3.Name == (string)item2.Value))))
            {
                var foundCustomVariable = _dynamicEntityState.CustomVariablesSave.First(item => item.Name == member);
                var foundStateCategory = _dynamicEntityState.StateCategoryList.First(item => foundCustomVariable.Properties.Any(item2 => item2.Name == "Type" && (string)item2.Value == item.Name));
                var foundState = foundStateCategory.States.First(item => item.Name == (string)convertedValue);

                foreach (var instruction in foundState.InstructionSaves)
                {
                    var newConvertedValue = ValueConverter.ConvertValue(instruction, this._dynamicEntityState.EntitySave);
                    convertedValue = ValueConverter.ConvertForProperty(newConvertedValue, instruction.Type, this.GetType().Name);
                    this.SetVariable(instruction.Member, newConvertedValue);
                }
            }
            else
            {
                ScreenManager.CurrentScreen.ApplyVariable(member, convertedValue, this);
            }
        }

        internal void AddToManagers(Layer layerToAddTo)
        {
            LayerProvidedByContainer = layerToAddTo;
            FlatRedBall.SpriteManager.AddPositionedObject(this);

            for (var i = _instancedObjects.Count - 1; i > -1; i--)
            {
                var obj = _instancedObjects[i];

                InstanceAddToManager.AddToManager(obj, _instancedObjects, layerToAddTo);
            }
        }

        public void ActivityEditMode()
        {
            if(ActivityEditModeEvent != null)
                ActivityEditModeEvent(this);
        }

        public object PropertyFinder(string name1)
        {
            var foundItem = _instancedObjects.Where(item => item.Name == name1).FirstOrDefault();

            if (foundItem != null)
                return foundItem.Value;

            return null;
        }
    }
}
