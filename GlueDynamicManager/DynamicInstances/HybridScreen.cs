using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlatRedBall.Screens;
using GlueDynamicManager.DynamicInstances.Containers;

namespace GlueDynamicManager.DynamicInstances
{
    internal class HybridScreen : HybridGlueElement
    {
        public HybridScreen(Screen screen) : base(screen)
        {
        }

        public Screen Screen {  get { return (Screen)GlueElement; } }
    }
}
