using Newtonsoft.Json;

namespace Mqttservice.Models
{
    public class Position
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
        [JsonProperty("carIndex")]
        public int CarIndex { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; } = "POSITION";
        [JsonProperty("value")]
        public int Value { get; set; }
    }
}
