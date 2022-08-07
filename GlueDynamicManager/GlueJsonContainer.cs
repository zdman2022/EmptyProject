using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace EmptyProject.GlueDynamicManager
{
    internal class GlueJsonContainer
    {
        public JToken Glue { get; set; }
        public Dictionary<string, JToken> Entities { get; set; } = new Dictionary<string, JToken>();
        public Dictionary<string, JToken> Screens { get; set; } = new Dictionary<string, JToken>();
    }
}
