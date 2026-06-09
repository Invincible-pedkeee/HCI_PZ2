using System;

namespace NetworkService.Model
{
    public class HistoryAction
    {
        public DateTime Timestamp { get; set; }

        public string Description { get; set; }

        public HistoryAction()
        {
        }

        public HistoryAction(string description)
        {
            Timestamp = DateTime.Now;
            Description = description;
        }

        public override string ToString()
        {
            return "[" + Timestamp.ToString("HH:mm:ss") + "] " + Description;
        }
    }
}