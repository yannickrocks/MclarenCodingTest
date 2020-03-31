using MQTTnet.Client;
using MQTTnet.Client.Publishing;
using Mqttservice.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mqttservice.Helpers.Interfaces
{
    public interface IPublishMetrics
    {
        Task<MqttClientPublishReasonCode> Speed(IMqttClient client, CarCoordinates car, Dictionary<int, PastAndCurrentData> carCoordinates);

        Task<MqttClientPublishReasonCode> EventHappened(IMqttClient client, Dictionary<int, PastAndCurrentData> carCoordinates, PositionResult results);

        Task<PositionResult> Positions(IMqttClient client, CarCoordinates car, Dictionary<int, PastAndCurrentData> carCoordinates);
    }
}
