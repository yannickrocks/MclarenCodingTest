using Mqttservice.Models;

namespace Mqttservice.Helpers.Interfaces
{
    public interface ICalculateSpeedAndDistance
    {
        public int GetSpeed(CarCoordinates previousCarData, CarCoordinates currentCarData);

        public double GetDistance(CarCoordinates previousCarData, CarCoordinates currentCarData);
    }
}
