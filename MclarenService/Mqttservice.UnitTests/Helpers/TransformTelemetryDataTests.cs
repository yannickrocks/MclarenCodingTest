using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using MQTTnet.Client;
using MQTTnet.Client.Publishing;
using Mqttservice.Helpers;
using Mqttservice.Helpers.Interfaces;
using Mqttservice.Models;
using Xunit;

namespace Mqttservice.UnitTests.Helpers {
    public class TransformTelemetryDataTests {
        readonly CarCoordinates car;
        readonly Dictionary<int, PastAndCurrentData> carCoordinates;
        readonly Mock<IMqttClient> client;
        readonly Mock<ILogger<TransformTelemetryData>> logger;
        readonly Mock<IPublishMetrics> publishMetrics;
        readonly TransformTelemetryData sut;

        public TransformTelemetryDataTests() {
            publishMetrics = new Mock<IPublishMetrics>();
            logger = new Mock<ILogger<TransformTelemetryData>>();
            client = new Mock<IMqttClient>();
            sut = new TransformTelemetryData(logger.Object, publishMetrics.Object);

            car = new CarCoordinates {
                CarIndex = 1,
                Timestamp = 1585498410875,
                Location = new Location {
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

        [Fact]
        public void ProcessNewMessageCarDoesNotExistAddToDictionaryReturnsSuccess() {
            var newCar = new CarCoordinates {
                CarIndex = 2,
                Timestamp = 1585498410875,
                Location = new Location {
                    Long = -1.022576453662667,
                    Lat = 52.06899250777391
                },
                TotalDistanceTraveled = 10
            };

            sut.ProcessNewMessage(client.Object, newCar, carCoordinates);

            logger.Verify(
                m => m.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => string.Equals(
                            "Car 2 has joined the race",
                            o.ToString(),
                            StringComparison.InvariantCultureIgnoreCase)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public void ProcessNewMessageCarDoesExistAndNoEventReturnsSuccessLogged() {
            var car = new CarCoordinates {
                CarIndex = 1,
                Timestamp = 1585498410875,
                Location = new Location {
                    Long = -1.022576453662667,
                    Lat = 52.06899250777391
                },
                TotalDistanceTraveled = 10
            };

            publishMetrics.Setup(x => x.Speed(client.Object, car, carCoordinates)).ReturnsAsync(MqttClientPublishReasonCode.Success);
            publishMetrics.Setup(x => x.Positions(client.Object, car, carCoordinates)).ReturnsAsync(
                new PositionResult {
                    ReasonCode = MqttClientPublishReasonCode.Success,
                    HasCarOvertaken = false
                });

            sut.ProcessNewMessage(client.Object, car, carCoordinates);

            logger.Verify(
                m => m.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => string.Equals(
                            "Car 1 has been processed",
                            o.ToString(),
                            StringComparison.InvariantCultureIgnoreCase)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public void ProcessNewMessageCarDoesExistAndAnEventHasHappenedReturnsSuccessLogged() {
            publishMetrics.Setup(x => x.Speed(client.Object, car, carCoordinates)).ReturnsAsync(MqttClientPublishReasonCode.Success);
            publishMetrics.Setup(x => x.Positions(client.Object, car, carCoordinates)).ReturnsAsync(
                new PositionResult {
                    ReasonCode = MqttClientPublishReasonCode.Success,
                    HasCarOvertaken = true,
                    CurrentCar = 1,
                    CarBehind = 2
                });

            sut.ProcessNewMessage(client.Object, car, carCoordinates);

            logger.Verify(
                m => m.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => string.Equals(
                            "A new event has happened",
                            o.ToString(),
                            StringComparison.InvariantCultureIgnoreCase)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);

            logger.Verify(
                m => m.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (o, t) => string.Equals(
                            "Car 1 has been processed",
                            o.ToString(),
                            StringComparison.InvariantCultureIgnoreCase)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}
