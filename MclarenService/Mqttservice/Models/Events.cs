using Newtonsoft.Json;

namespace Mqttservice.Models
{
    public class Events
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
