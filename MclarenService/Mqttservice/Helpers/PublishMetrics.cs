using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.Client.Publishing;
using Mqttservice.Helpers.Interfaces;
using Mqttservice.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mqttservice.Helpers
{
    public class PublishMetrics : IPublishMetrics
    {
        readonly ICalculateSpeedAndDistance calculate;
        readonly ILogger<PublishMetrics> logger;
        readonly IPublishMessageToMqtt publishToMqtt;

        public PublishMetrics(IPublishMessageToMqtt publishToMqtt, ICalculateSpeedAndDistance calculate, ILogger<PublishMetrics> logger)
        {
            this.publishToMqtt = publishToMqtt;
            this.calculate = calculate;
            this.logger = logger;
        }

        public async Task<MqttClientPublishReasonCode> Speed(IMqttClient client, CarCoordinates car, Dictionary<int, PastAndCurrentData> carCoordinates)
        {
            var speedMessage = new Speed
            {
                CarIndex = car.CarIndex,
                Timestamp = car.Timestamp,
                Value = calculate.GetSpeed(
                    carCoordinates[car.CarIndex].previous,
                    carCoordinates[car.CarIndex].current)
            };

            var publishedCarStatus = await publishToMqtt.CarStatus(client, JsonConvert.SerializeObject(speedMessage));

            if (publishedCarStatus.ReasonCode != MqttClientPublishReasonCode.Success)
            {
                logger.LogError($"Publishing CarStatus for Speed failed with this code : {publishedCarStatus.ReasonCode.ToString()}");
                return publishedCarStatus.ReasonCode;
            }

            return publishedCarStatus.ReasonCode;
        }

        public async Task<MqttClientPublishReasonCode> EventHappened(IMqttClient client, Dictionary<int, PastAndCurrentData> carCoordinates, PositionResult results)
        {
            var carHasOvertaken = new Events
            {
                Timestamp = carCoordinates[results.CurrentCar].current.Timestamp,
                Text = $"Car {results.CurrentCar} races ahead of Car {results.CarBehind} in a dramatic overtake."
            };

            var publishedEvent = await publishToMqtt.Events(client, JsonConvert.SerializeObject(carHasOvertaken));

            if (publishedEvent.ReasonCode != MqttClientPublishReasonCode.Success)
            {
                logger.LogError($"Publishing Event failed with this code : {publishedEvent.ReasonCode.ToString()}");
                return publishedEvent.ReasonCode;
            }

            return MqttClientPublishReasonCode.Success;
        }

        public async Task<PositionResult> Positions(IMqttClient client, CarCoordinates car, Dictionary<int, PastAndCurrentData> carCoordinates)
        {
            var positionChanged = new PositionResult
            {
                ReasonCode = MqttClientPublishReasonCode.Success
            };

            if (carCoordinates[car.CarIndex].previous != null)
            {
                carCoordinates[car.CarIndex].current.TotalDistanceTraveled =
                    carCoordinates[car.CarIndex].previous.TotalDistanceTraveled + calculate.GetDistance(
                        carCoordinates[car.CarIndex].previous,
                        carCoordinates[car.CarIndex].current
                    );
            }

            var updatePositions =
                carCoordinates.ToDictionary(k => k.Key, v => v.Value.current.TotalDistanceTraveled);
            var currentRankOrder = updatePositions.OrderBy(x => x.Value).Select(x => x.Key).Reverse()
                .ToList();

            for (var i = 0; i < currentRankOrder.Count; i++)
            {
                if (car.CarIndex != currentRankOrder[i])
                {
                    continue;
                }

                var position = i + 1;

                var positionMessage = new Position
                {
                    Timestamp = carCoordinates[currentRankOrder[i]].current.Timestamp,
                    CarIndex = currentRankOrder[i],
                    Value = position
                };

                var publishedCarStatus = await publishToMqtt.CarStatus(client, JsonConvert.SerializeObject(positionMessage));

                if (publishedCarStatus.ReasonCode != MqttClientPublishReasonCode.Success)
                {
                    logger.LogError($"Publishing CarStatus for Position failed with this code : {publishedCarStatus.ReasonCode.ToString()}");
                    positionChanged.ReasonCode = publishedCarStatus.ReasonCode;
                    return positionChanged;
                }

                carCoordinates[currentRankOrder[i]].current.Position = position;

                if (carCoordinates[currentRankOrder[i]].current.Position < carCoordinates[currentRankOrder[i]].previous.Position)
                {
                    positionChanged.HasCarOvertaken = true;
                    positionChanged.CurrentCar = currentRankOrder[i];
                    positionChanged.CarBehind = currentRankOrder[i + 1];
                }
            }

            return positionChanged;
        }
    }
}
