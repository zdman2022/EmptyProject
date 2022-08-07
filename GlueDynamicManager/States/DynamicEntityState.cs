using GlueControl.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlueDynamicManager.States
{
    public class DynamicEntityState
    {
        public EntitySave EntitySave { get; internal set; }
        public List<CustomVariable> CustomVariablesSave { get; internal set; }
        public List<StateSaveCategory> StateCategoryList { get; internal set; }
    }
}
