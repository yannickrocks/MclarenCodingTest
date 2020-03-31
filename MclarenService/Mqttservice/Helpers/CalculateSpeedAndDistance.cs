using Mqttservice.Helpers.Interfaces;
using Mqttservice.Models;
using System.Device.Location;

namespace Mqttservice.Helpers
{
    public class CalculateSpeedAndDistance : ICalculateSpeedAndDistance
    {

        public int GetSpeed(CarCoordinates previousCarData, CarCoordinates currentCarData)
        {
            var distance = GetDistance(previousCarData, currentCarData);
            var time = (currentCarData.Timestamp - previousCarData.Timestamp) / 1000.00;

            var speed = distance / time;

            return (int)speed;
        }

        public double GetDistance(CarCoordinates previousCarData, CarCoordinates currentCarData)
        {
            var previousPosition = new GeoCoordinate(previousCarData.Location.Lat, previousCarData.Location.Long);
            var currentPosition = new GeoCoordinate(currentCarData.Location.Lat, currentCarData.Location.Long);

            return previousPosition.GetDistanceTo(currentPosition);
        }
    }
}
