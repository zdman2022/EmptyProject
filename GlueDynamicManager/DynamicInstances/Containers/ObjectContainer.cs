using FlatRedBall.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmptyProject.GlueDynamicManager.DynamicInstances.Containers
{
    internal class ObjectContainer : BaseContainer<object>
    {
        public string ObjectType => NamedObjectSave.SourceClassType;
    }
}
