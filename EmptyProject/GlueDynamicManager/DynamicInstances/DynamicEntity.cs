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
            for (var i = _dynamicEntityState.EntitySave.NamedObjects.Count - 1; i > -1; i--)
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
                        Value = InstanceInstantiator.Instantiate(no.SourceClassType)
                    });
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

                if(obj.ObjectType == "FlatRedBall.Math.Geometry.Circle")
                {
                    FlatRedBall.Math.Geometry.ShapeManager.AddToLayer((FlatRedBall.Math.Geometry.Circle)obj.Value, LayerProvidedByContainer);
                }
            }
        }
    }
}
