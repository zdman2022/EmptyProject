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
using GlueControl.Models;
using FlatRedBall.Content.Instructions;

namespace GlueDynamicManager.DynamicInstances
{
    public class DynamicEntity : PositionedObject, ICollidable, IDestroyable, IEntity, IDynamic
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

        public HashSet<string> ItemsCollidedAgainst { get; private set; } = new HashSet<string>();

        public HashSet<string> LastFrameItemsCollidedAgainst { get; private set; } = new HashSet<string>();
        public string TypeName => this.ElementNameGame;

        public bool IsLoaded { get; set; }

        private EntityState _dynamicEntityState;
        protected FlatRedBall.Graphics.Layer LayerProvidedByContainer = null;
        private List<ObjectContainer> _dynamicProperties = new List<ObjectContainer>();

        public DynamicEntity(string nameGlue, EntityState dynamicEntityState)
        {
            ElementNameGame = CommandReceiver.GlueToGameElementName(nameGlue);
            _dynamicEntityState = dynamicEntityState;
            GlueDynamicManager.Self.AttachEntity(this, true);
            InitializeEntity();
            mGeneratedCollision = new FlatRedBall.Math.Geometry.ShapeCollection();
        }

        private void InitializeEntity()
        {
            //for (var i = 0; i < _dynamicEntityState.EntitySave.NamedObjects.Count; i++)
            //{
            //    var no = _dynamicEntityState.EntitySave.NamedObjects[i];
            //    if (GlueDynamicManager.Self.ElementIsDynamic(no.SourceClassType))
            //    {
            //        throw new NotImplementedException();
            //    }
            //    else
            //    {
            //        _instancedObjects.Add(new ObjectContainer
            //        {
            //            NamedObjectSave = no,
            //            Value = InstanceInstantiator.Instantiate(no, this),
            //            CombinedInstructionSaves = no.InstructionSaves
            //        });
            //    }
            //}

            //for (var i = 0; i < _instancedObjects.Count; i++)
            //{
            //    var obj = _instancedObjects[i];

            //    foreach (var instruction in obj.CombinedInstructionSaves)
            //    {
            //        var convertedValue = ValueConverter.ConvertValue(instruction, this._dynamicEntityState.EntitySave);
            //        convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, obj.ObjectType);
            //        FlatRedBall.Screens.ScreenManager.CurrentScreen.ApplyVariable(instruction.Member, convertedValue, obj.Value);
            //    }

            //    if (obj.Value is PositionedObject po)
            //    {
            //        po.CopyAbsoluteToRelative();
            //        po.AttachTo(this, false);
            //    }
            //}

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
            for (int i = _dynamicProperties.Count - 1; i > -1; i--)
            {
                InstanceDestroy.Destroy(_dynamicProperties[i]);
            }

            mGeneratedCollision.RemoveFromManagers(clearThis: false);

            if (DestroyEvent != null)
                DestroyEvent(this);

            this.RemoveSelfFromListsBelongingTo();
        }

        //internal void SetVariable(string member, object convertedValue)
        //{
        //    if (_dynamicEntityState.CustomVariablesSave.Where(item => item.SourceObject != null).Any(item => item.Name == member))
        //    {
        //        var foundCustomVariable = _dynamicEntityState.CustomVariablesSave.First(item => item.Name == member);
        //        var foundObject = _dynamicProperties.Where(item => item.Name == foundCustomVariable.SourceObject).First();

        //        ScreenManager.CurrentScreen.ApplyVariable(foundCustomVariable.SourceObjectProperty, convertedValue, foundObject.Value);
        //    }
        //    else if (_dynamicEntityState.CustomVariablesSave.Any(item => item.Name == member && item.Properties.Any(item2 => item2.Name == "Type" && _dynamicEntityState.StateCategoryList.Any(item3 => item3.Name == (string)item2.Value))))
        //    {
        //        var foundCustomVariable = _dynamicEntityState.CustomVariablesSave.First(item => item.Name == member);
        //        var foundStateCategory = _dynamicEntityState.StateCategoryList.First(item => foundCustomVariable.Properties.Any(item2 => item2.Name == "Type" && (string)item2.Value == item.Name));
        //        var foundState = foundStateCategory.States.First(item => item.Name == (string)convertedValue);

        //        foreach (var instruction in foundState.InstructionSaves)
        //        {
        //            var newConvertedValue = ValueConverter.ConvertValue(instruction, this._dynamicEntityState.EntitySave);
        //            convertedValue = ValueConverter.ConvertForProperty(newConvertedValue, instruction.Type, this.GetType().Name);
        //            this.SetVariable(instruction.Member, newConvertedValue);
        //        }
        //    }
        //    else
        //    {
        //        ScreenManager.CurrentScreen.ApplyVariable(member, convertedValue, this);
        //    }
        //}

        internal void AddToManagers(Layer layerToAddTo)
        {
            LayerProvidedByContainer = layerToAddTo;
            FlatRedBall.SpriteManager.AddPositionedObject(this);

            for (var i = 0; i < _dynamicProperties.Count; i++)
            {
                var obj = _dynamicProperties[i];

                InstanceAddToManager.AddToManager(obj, _dynamicProperties, layerToAddTo);
            }
        }

        public void ActivityEditMode()
        {
            if(ActivityEditModeEvent != null)
                ActivityEditModeEvent(this);
        }

        public object GetPropertyValue(string name1)
        {
            var foundItem = _dynamicProperties.Where(item => item.Name == name1).FirstOrDefault();

            if (foundItem != null)
                return foundItem.Value;

            return null;
        }

        public bool SetPropertyValue(string name, object value, NamedObjectSave nos, List<InstructionSave> instructionSaves)
        {
            var foundItem = _dynamicProperties.Where(item => item.Name == name).FirstOrDefault();

            if (foundItem != null)
                _dynamicProperties.Remove(foundItem);
                
            if(value != null)
                _dynamicProperties.Add(
                    value is ObjectContainer ocValue
                    ? ocValue
                    : new ObjectContainer(name)
                        {
                            Value = value,
                            NamedObjectSave = nos,
                            CombinedInstructionSaves = instructionSaves
                        }
                );

            return true;
        }

        public bool CallMethodIfExists(string methodName, object[] args, out object returnValue)
        {
            returnValue = null;
            return false;
        }
    }
}
