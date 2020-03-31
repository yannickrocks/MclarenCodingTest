using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Publishing;
using Mqttservice.Helpers.Interfaces;
using System.Threading.Tasks;

namespace Mqttservice.Helpers
{
    public class PublishMessageToMqtt : IPublishMessageToMqtt
    {
        public async Task<MqttClientPublishResult> CarStatus(IMqttClient client, string carStatus)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("carStatus")
                .WithPayload(carStatus)
                .Build();

            return await client.PublishAsync(message);
        }

        public async Task<MqttClientPublishResult> Events(IMqttClient client, string events)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("events")
                .WithPayload(events)
                .Build();

            return await client.PublishAsync(message);
        }
    }
}
