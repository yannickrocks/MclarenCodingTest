namespace Mqttservice.Models
{
    public class CarCoordinates
    {
        public long Timestamp { get; set; }
        public int CarIndex { get; set; }
        public Location Location { get; set; }
        public double TotalDistanceTraveled { get; set; }
        public int Position { get; set; }
    }
}
