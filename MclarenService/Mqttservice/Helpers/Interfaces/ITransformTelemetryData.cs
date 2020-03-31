using MQTTnet.Client;
using Mqttservice.Models;
using System.Collections.Generic;

namespace Mqttservice.Helpers.Interfaces
{
    public interface ITransformTelemetryData
    {
        void ProcessNewMessage(IMqttClient client, CarCoordinates car, Dictionary<int, PastAndCurrentData> carCoordinates);
    }
}
