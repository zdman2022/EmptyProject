using FlatRedBall.Content.Instructions;
using FlatRedBall.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmptyProject.GlueDynamicManager.DynamicInstances.Containers
{
    internal class DynamicEntityContainer : BaseContainer<DynamicEntity>
    {
        public List<InstructionSave> InstructionSaves { get; internal set; }
    }
}
