using System.Reflection;
using FluentAssertions;
using Mqttservice.Helpers;
using Mqttservice.Models;
using Xunit;

namespace Mqttservice.UnitTests.Helpers {
    public class CalculateSpeedAndDistanceTests {
        readonly CarCoordinates currentCarData;
        readonly MethodInfo getDistance = typeof(CalculateSpeedAndDistance).GetMethod(
            nameof(CalculateSpeedAndDistance.GetDistance));
        readonly MethodInfo getSpeed = typeof(CalculateSpeedAndDistance).GetMethod(
            nameof(CalculateSpeedAndDistance.GetSpeed));
        readonly CarCoordinates previousCarData;
        readonly CalculateSpeedAndDistance sut;

        public CalculateSpeedAndDistanceTests() {
            sut = new CalculateSpeedAndDistance();
            previousCarData = new CarCoordinates {
                Timestamp = 1585498410670,
                Location = new Location {
                    Long = -1.0227506693058277,
                    Lat = 52.06885015666453
                }
            };

            currentCarData = new CarCoordinates {
                Timestamp = 1585498410875,
                Location = new Location {
                    Long = -1.022576453662667,
                    Lat = 52.06899250777391
                }
            };
        }

        [Fact]
        public void GetDistanceReturnsInt() {
            getDistance.ReturnType.Should().Be<double>();
        }

        [Fact]
        public void GetSpeedReturnsInt() {
            getSpeed.ReturnType.Should().Be<int>();
        }

        [Fact]
        public void GetDistanceReturnsExpectedResult() {
            var expectedDistance = 19.824993641643324;

            var result = sut.GetDistance(previousCarData, currentCarData);

            result.Should().Be(expectedDistance);
        }

        [Fact]
        public void GetSpeedReturnsExpectedResult() {
            var expectedSpeed = 96;

            var result = sut.GetSpeed(previousCarData, currentCarData);

            result.Should().Be(expectedSpeed);
        }
    }
}
