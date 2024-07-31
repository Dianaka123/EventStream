using Newtonsoft.Json;
using System;

namespace Assets.Scripts
{
    [Serializable]
    public class EventData
    {
        [JsonProperty("type")]
        public string EventType;

        [JsonProperty("data")]
        public string Data;
    }
}
