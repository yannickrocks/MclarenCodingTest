using Newtonsoft.Json;

namespace Mqttservice.Models
{
    public class Speed
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
        [JsonProperty("carIndex")]
        public int CarIndex { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; } = "SPEED";
        [JsonProperty("value")]
        public int Value { get; set; }
    }
}
