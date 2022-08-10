using GlueControl.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace GlueDynamicManager
{
    internal class GlueJsonContainer
    {
        public JsonContainer<GlueProjectSave> Glue { get; set; }
        public Dictionary<string, JsonContainer<EntitySave>> Entities { get; set; } = new Dictionary<string, JsonContainer<EntitySave>>();
        public Dictionary<string, JsonContainer<ScreenSave>> Screens { get; set; } = new Dictionary<string, JsonContainer<ScreenSave>>();

        public class JsonContainer<T>
        {
            public JsonContainer(string json)
            {
                Json = JToken.Parse(json);
                Value = (T)JsonConvert.DeserializeObject(json, typeof(T));
            }

            public JToken Json { get; set; }
            public T Value { get; set; }
        }

        internal GlueProjectSave GetFullClone()
        {
            var returnValue = JsonConvert.DeserializeObject<GlueProjectSave>(Glue.Json.ToString());

            returnValue.Entities = Entities.Values.Select(item => JsonConvert.DeserializeObject<EntitySave>(item.Json.ToString())).ToList();
            returnValue.Screens = Screens.Values.Select(item => JsonConvert.DeserializeObject<ScreenSave>(item.Json.ToString())).ToList();

            return returnValue;
        }
    }
}
