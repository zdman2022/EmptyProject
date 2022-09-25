using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlatRedBall.Screens;
using GlueControl;
using GlueDynamicManager.DynamicInstances.Containers;

namespace GlueDynamicManager.DynamicInstances
{
    internal class HybridScreen : HybridGlueElement
    {
        public static string CurrentScreenGlue { get; internal set; }

        public HybridScreen(Screen screen) : base(screen)
        {
        }

        public Screen Screen {  get { return (Screen)GlueElement; } }

        public override string TypeName
        {
            get
            {
                if(CurrentScreenGlue != null)
                {
                    return CommandReceiver.GlueToGameElementName(CurrentScreenGlue);
                }
                else
                {
                    return base.TypeName;
                }
            }
        }
    }
}
