using MQTTnet.Client;
using MQTTnet.Client.Publishing;
using System.Threading.Tasks;

namespace Mqttservice.Helpers.Interfaces
{
    public interface IPublishMessageToMqtt
    {
        Task<MqttClientPublishResult> CarStatus(IMqttClient client, string carStatus);

        Task<MqttClientPublishResult> Events(IMqttClient client, string events);
    }
}
