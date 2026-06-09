using System;

namespace NetworkService.Model
{
    public class Measurement
    {
        public int EntityId { get; set; }

        public double Value { get; set; }

        public DateTime Timestamp { get; set; }

        public bool IsValid { get; set; }

        public Measurement()
        {
        }

        public Measurement(int entityId, double value, DateTime timestamp, bool isValid)
        {
            EntityId = entityId;
            Value = value;
            Timestamp = timestamp;
            IsValid = isValid;
        }
    }
}