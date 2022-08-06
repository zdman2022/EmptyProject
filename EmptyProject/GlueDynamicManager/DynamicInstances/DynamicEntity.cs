﻿using EmptyProject.GlueDynamicManager.Converters;
using EmptyProject.GlueDynamicManager.DynamicInstances.Containers;
using EmptyProject.GlueDynamicManager.States;
using FlatRedBall;
using FlatRedBall.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmptyProject.GlueDynamicManager.DynamicInstances
{
    internal class DynamicEntity : PositionedObject
    {
        private DynamicEntityState _dynamicEntityState;
        protected FlatRedBall.Graphics.Layer LayerProvidedByContainer = null;
        private List<ObjectContainer> _instancedObjects = new List<ObjectContainer>();

        public DynamicEntity(DynamicEntityState dynamicEntityState)
        {
            _dynamicEntityState = dynamicEntityState;
            InitializeEntity();
        }

        private void InitializeEntity()
        {
            for (var i = 0; i < _dynamicEntityState.EntitySave.NamedObjects.Count ; i++)
            {
                var no = _dynamicEntityState.EntitySave.NamedObjects[i];
                if (GlueDynamicManager.Self.EntityIsDynamic(no.SourceClassType))
                {
                    throw new NotImplementedException();
                }
                else
                {
                    _instancedObjects.Add(new ObjectContainer
                    {
                        Name = no.InstanceName,
                        ObjectType = no.SourceClassType,
                        Value = InstanceInstantiator.Instantiate(no.SourceClassType),
                        InstructionSaves = no.InstructionSaves
                    });
                }
            }

            for (var i = 0; i < _instancedObjects.Count; i++)
            {
                var obj = _instancedObjects[i];

                foreach(var instruction in obj.InstructionSaves)
                {
                    var convertedValue = ValueConverter.ConvertValue(instruction, this._dynamicEntityState.EntitySave);
                    convertedValue = ValueConverter.ConvertForProperty(convertedValue, instruction.Type, obj.ObjectType);
                    FlatRedBall.Screens.ScreenManager.CurrentScreen.ApplyVariable(instruction.Member, convertedValue, obj.Value);
                }
            }
        }

        public void Activity()
        {
        }

        internal void Destroy()
        {
        }

        internal void AddToManagers(Layer layerToAddTo)
        {
            LayerProvidedByContainer = layerToAddTo;
            FlatRedBall.SpriteManager.AddPositionedObject(this);

            for(var i = _instancedObjects.Count - 1; i > -1; i--)
            {
                var obj = _instancedObjects[i];

                if(ShapeManagerHandler.IsShape(obj.ObjectType))
                {
                    ShapeManagerHandler.AddToLayer(obj.Value, LayerProvidedByContainer, obj.ObjectType);
                }
            }
        }
    }
}
