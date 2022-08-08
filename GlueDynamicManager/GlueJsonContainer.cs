using GlueControl.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

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
    }
}
