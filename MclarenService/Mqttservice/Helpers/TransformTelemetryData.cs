using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.Client.Publishing;
using Mqttservice.Helpers.Interfaces;
using Mqttservice.Models;
using System.Collections.Generic;

namespace Mqttservice.Helpers
{
    public class TransformTelemetryData : ITransformTelemetryData
    {
        readonly ILogger<TransformTelemetryData> logger;
        readonly IPublishMetrics publishMetrics;

        public TransformTelemetryData(ILogger<TransformTelemetryData> logger, IPublishMetrics publishMetrics)
        {
            this.logger = logger;
            this.publishMetrics = publishMetrics;
        }

        public async void ProcessNewMessage(IMqttClient client, CarCoordinates car, Dictionary<int, PastAndCurrentData> carCoordinates)
        {
            if (!carCoordinates.ContainsKey(car.CarIndex))
            {
                carCoordinates.Add(
                    car.CarIndex,
                    new PastAndCurrentData
                    {
                        previous = car,
                        current = new CarCoordinates()
                    });

                logger.LogInformation($"Car {car.CarIndex} has joined the race");
            }
            else
            {
                carCoordinates[car.CarIndex].current = car;

                var publishedSpeed = await publishMetrics.Speed(client, car, carCoordinates);
                var publishedPosition = await publishMetrics.Positions(client, car, carCoordinates);

                if (publishedPosition.HasCarOvertaken)
                {
                    var publishedEvent = await publishMetrics.EventHappened(client, carCoordinates, publishedPosition);
                    if (publishedEvent == MqttClientPublishReasonCode.Success)
                    {
                        logger.LogInformation("A new event has happened");
                    }
                }

                carCoordinates[car.CarIndex].previous =
                    carCoordinates[car.CarIndex].current;

                if (publishedSpeed == MqttClientPublishReasonCode.Success && publishedPosition.ReasonCode == MqttClientPublishReasonCode.Success)
                {
                    logger.LogInformation($"Car {car.CarIndex} has been processed");
                }
            }
        }
    }
}
