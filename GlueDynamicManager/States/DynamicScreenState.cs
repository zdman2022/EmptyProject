using GlueControl.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmptyProject.GlueDynamicManager.States
{
    internal class DynamicScreenState
    {
        public ScreenSave ScreenSave { get; internal set; }
        public ScreenSave BaseScreenSave { get; internal set; }
    }
}
