using MQTTnet.Client.Publishing;

namespace Mqttservice.Models
{
    public class PositionResult
    {
        public MqttClientPublishReasonCode ReasonCode { get; set; }
        public bool HasCarOvertaken { get; set; } = false;
        public int CurrentCar { get; set; }
        public int CarBehind { get; set; }
    }
}
