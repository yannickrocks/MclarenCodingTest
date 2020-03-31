using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MQTTnet.Client;
using MQTTnet.Client.Publishing;
using Mqttservice.Helpers;
using Mqttservice.Helpers.Interfaces;
using Mqttservice.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xunit;

namespace Mqttservice.UnitTests.Helpers
{
    public class PublishMetricsTests
    {
        readonly Mock<ICalculateSpeedAndDistance> calculate;
        readonly Mock<IMqttClient> client;
        readonly Mock<ILogger<PublishMetrics>> logger;
        readonly Mock<IPublishMessageToMqtt> publishToMqtt;
        readonly PublishMetrics sut;
        CarCoordinates car;
        Dictionary<int, PastAndCurrentData> carCoordinates;

        public PublishMetricsTests()
        {
            calculate = new Mock<ICalculateSpeedAndDistance>();
            publishToMqtt = new Mock<IPublishMessageToMqtt>();
            logger = new Mock<ILogger<PublishMetrics>>();
            client = new Mock<IMqttClient>();
            sut = new PublishMetrics(publishToMqtt.Object, calculate.Object, logger.Object);
            SetupTestData();
        }

        [Fact]
        public async void PublishSpeedIsSuccessful()
        {
            var speedMessage = new Speed
            {
                CarIndex = 1,
                Timestamp = car.Timestamp,
                Type = "SPEED",
                Value = 96
            };

            calculate.Setup(
                x => x.GetSpeed(
                    carCoordinates[car.CarIndex].previous,
                    carCoordinates[car.CarIndex].current)).Returns(96);

            publishToMqtt
                .Setup(x => x.CarStatus(client.Object, JsonConvert.SerializeObject(speedMessage)))
                .ReturnsAsync(
                    new MqttClientPublishResult
                    {
                        ReasonCode = MqttClientPublishReasonCode.Success
                    });

            var result = await sut.Speed(client.Object, car, carCoordinates);

            result.Should().BeEquivalentTo(MqttClientPublishReasonCode.Success);
        }

        [Fact]
        public async void PublishSpeedLogsError()
        {
            var speedMessage = new Speed
            {
                CarIndex = 1,
                Timestamp = car.Timestamp,
                Type = "SPEED",
                Value = 96
            };

            calculate.Setup(
                x => x.GetSpeed(
                    carCoordinates[car.CarIndex].previous,
                    carCoordinates[car.CarIndex].current)).Returns(96);

            publishToMqtt
                .Setup(x => x.CarStatus(client.Object, JsonConvert.SerializeObject(speedMessage)))
                .ReturnsAsync(
                    new MqttClientPublishResult
                    {
                        ReasonCode = MqttClientPublishReasonCode.UnspecifiedError
                    });

            var result = await sut.Speed(client.Object, car, carCoordinates);

            result.Should().BeEquivalentTo(MqttClientPublishReasonCode.UnspecifiedError);
            logger.Verify(
                m => m.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => string.Equals(
                            "Publishing CarStatus for Speed failed with this code : UnspecifiedError",
                            o.ToString(),
                            StringComparison.InvariantCultureIgnoreCase)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async void PublishPositionsIsSuccessful()
        {
            var expected = new PositionResult
            {
                ReasonCode = MqttClientPublishReasonCode.Success,
                HasCarOvertaken = false
            };

            var positionMessage = new Position
            {
                CarIndex = 1,
                Timestamp = car.Timestamp,
                Type = "POSITION",
                Value = 1
            };

            carCoordinates = new Dictionary<int, PastAndCurrentData> {
                {
                    1, new PastAndCurrentData {
                        previous = new CarCoordinates {
                            CarIndex = 1,
                            Timestamp = 1585498410670,
                            Location = new Location {
                                Long = -1.0227506693058277,
                                Lat = 52.06885015666453
                            },
                            Position = 1
                        },

                        current = new CarCoordinates {
                            Timestamp = 1585498410875,
                            Location = new Location {
                                Long = -1.022576453662667,
                                Lat = 52.06899250777391
                            },
                            Position = 0
                        }
                    }
                }
            };

            calculate.Setup(
                x => x.GetDistance(
                    carCoordinates[car.CarIndex].previous,
                    carCoordinates[car.CarIndex].current)).Returns(19.824993641643324);

            publishToMqtt
                .Setup(x => x.CarStatus(client.Object, JsonConvert.SerializeObject(positionMessage)))
                .ReturnsAsync(
                    new MqttClientPublishResult
                    {
                        ReasonCode = MqttClientPublishReasonCode.Success
                    });

            var result = await sut.Positions(client.Object, car, carCoordinates);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async void PublishPositionsLogsError()
        {
            var expected = new PositionResult
            {
                ReasonCode = MqttClientPublishReasonCode.UnspecifiedError,
                HasCarOvertaken = false
            };

            var positionMessage = new Position
            {
                CarIndex = 1,
                Timestamp = 1585498410875,
                Type = "POSITION",
                Value = 1
            };

            carCoordinates.Add(
                2,
                new PastAndCurrentData
                {
                    previous = new CarCoordinates
                    {
                        CarIndex = 1,
                        Timestamp = 1585498410670,
                        Location = new Location
                        {
                            Long = -1.0227506693058277,
                            Lat = 52.06885015666453
                        },
                        Position = 2
                    },
                    current = new CarCoordinates()
                });

            calculate.Setup(
                x => x.GetDistance(
                    carCoordinates[car.CarIndex].previous,
                    carCoordinates[car.CarIndex].current)).Returns(19.824993641643324);

            publishToMqtt
                .Setup(x => x.CarStatus(client.Object, JsonConvert.SerializeObject(positionMessage)))
                .ReturnsAsync(
                    new MqttClientPublishResult
                    {
                        ReasonCode = MqttClientPublishReasonCode.UnspecifiedError
                    });

            var result = await sut.Positions(client.Object, car, carCoordinates);

            result.Should().BeEquivalentTo(expected);

            logger.Verify(
                m => m.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => string.Equals(
                            "Publishing CarStatus for Position failed with this code : UnspecifiedError",
                            o.ToString(),
                            StringComparison.InvariantCultureIgnoreCase)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async void PublishEventIsSuccessful()
        {
            var expected = new PositionResult
            {
                ReasonCode = MqttClientPublishReasonCode.Success,
                HasCarOvertaken = true,
                CurrentCar = 1,
                CarBehind = 2
            };

            var eventMessage = new Events
            {
                Timestamp = 1585498410875,
                Text = "Car 1 races ahead of Car 2 in a dramatic overtake."
            };

            publishToMqtt.Setup(x => x.Events(client.Object, JsonConvert.SerializeObject(eventMessage))).ReturnsAsync(
                new MqttClientPublishResult
                {
                    ReasonCode = MqttClientPublishReasonCode.Success
                });

            var result = await sut.EventHappened(client.Object, carCoordinates, expected);

            result.Should().BeEquivalentTo(MqttClientPublishReasonCode.Success);
        }

        [Fact]
        public async void PublishEventHasHappenedLogsError()
        {
            var positionResult = new PositionResult
            {
                ReasonCode = MqttClientPublishReasonCode.Success,
                HasCarOvertaken = true,
                CurrentCar = 1,
                CarBehind = 2
            };

            var eventMessage = new Events
            {
                Timestamp = 1585498410875,
                Text = "Car 1 races ahead of Car 2 in a dramatic overtake."
            };

            publishToMqtt.Setup(x => x.Events(client.Object, JsonConvert.SerializeObject(eventMessage))).ReturnsAsync(
                new MqttClientPublishResult
                {
                    ReasonCode = MqttClientPublishReasonCode.UnspecifiedError
                });

            var result = await sut.EventHappened(client.Object, carCoordinates, positionResult);

            result.Should().BeEquivalentTo(MqttClientPublishReasonCode.UnspecifiedError);

            logger.Verify(
                m => m.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => string.Equals(
                            "Publishing Event failed with this code : UnspecifiedError",
                            o.ToString(),
                            StringComparison.InvariantCultureIgnoreCase)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        void SetupTestData()
        {
            car = new CarCoordinates
            {
                CarIndex = 1,
                Timestamp = 1585498410875,
                Location = new Location
                {
                    Long = -1.022576453662667,
                    Lat = 52.06899250777391
                },
                TotalDistanceTraveled = 10
            };

            carCoordinates = new Dictionary<int, PastAndCurrentData> {
                {
                    1, new PastAndCurrentData {
                        previous = new CarCoordinates {
                            CarIndex = 1,
                            Timestamp = 1585498410670,
                            Location = new Location {
                                Long = -1.0227506693058277,
                                Lat = 52.06885015666453
                            },
                            Position = 2
                        },

                        current = new CarCoordinates {
                            Timestamp = 1585498410875,
                            Location = new Location {
                                Long = -1.022576453662667,
                                Lat = 52.06899250777391
                            },
                            Position = 0
                        }
                    }
                }
            };
        }
    }
}
