using FlatRedBall.Content.Instructions;
using FlatRedBall.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlueDynamicManager.DynamicInstances.Containers
{
    internal class ObjectContainer : BaseContainer<object>
    {
        public ObjectContainer(string name)
        {
            _name = name;
        }
    }
}
