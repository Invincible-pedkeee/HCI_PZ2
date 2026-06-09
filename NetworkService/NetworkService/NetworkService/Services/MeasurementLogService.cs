using System;
using System.Globalization;
using System.IO;
using NetworkService.Model;

namespace NetworkService.Services
{
    public class MeasurementLogService
    {
        private readonly string logFilePath;

        public string LogFilePath
        {
            get
            {
                return logFilePath;
            }
        }

        public MeasurementLogService()
        {
            string logsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            if (!Directory.Exists(logsFolderPath))
            {
                Directory.CreateDirectory(logsFolderPath);
            }

            logFilePath = Path.Combine(logsFolderPath, "measurements.txt");
        }

        public void WriteMeasurement(Measurement measurement, NetworkEntity entity)
        {
            if (measurement == null || entity == null)
            {
                return;
            }

            string statusText = measurement.IsValid ? "VALID" : "INVALID";

            string line =
                measurement.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) +
                " | EntityId=" + measurement.EntityId +
                " | Name=" + entity.Name +
                " | Type=" + entity.Type.Name +
                " | Value=" + measurement.Value.ToString(CultureInfo.InvariantCulture) +
                " | Status=" + statusText;

            File.AppendAllText(logFilePath, line + Environment.NewLine);
        }
    }
}