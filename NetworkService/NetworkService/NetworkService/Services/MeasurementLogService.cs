using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
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
            if (measurement == null || entity == null || entity.Type == null)
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

        public ObservableCollection<Measurement> ReadLastMeasurementsForEntity(int entityId, int count)
        {
            List<Measurement> measurements = ReadAllMeasurements()
                .Where(measurement => measurement.EntityId == entityId)
                .OrderByDescending(measurement => measurement.Timestamp)
                .Take(count)
                .OrderBy(measurement => measurement.Timestamp)
                .ToList();

            return new ObservableCollection<Measurement>(measurements);
        }

        public List<Measurement> ReadAllMeasurements()
        {
            List<Measurement> measurements = new List<Measurement>();

            if (!File.Exists(logFilePath))
            {
                return measurements;
            }

            string[] lines = File.ReadAllLines(logFilePath);

            foreach (string line in lines)
            {
                Measurement measurement;

                if (TryParseMeasurementLine(line, out measurement))
                {
                    measurements.Add(measurement);
                }
            }

            return measurements;
        }

        private bool TryParseMeasurementLine(string line, out Measurement measurement)
        {
            measurement = null;

            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            string[] parts = line.Split('|');

            if (parts.Length < 6)
            {
                return false;
            }

            DateTime timestamp;
            int entityId;
            double value;
            bool isValid;

            if (!DateTime.TryParseExact(
                    parts[0].Trim(),
                    "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out timestamp))
            {
                return false;
            }

            if (!TryReadIntPart(parts[1], "EntityId", out entityId))
            {
                return false;
            }

            if (!TryReadDoublePart(parts[4], "Value", out value))
            {
                return false;
            }

            if (!TryReadStatusPart(parts[5], out isValid))
            {
                return false;
            }

            measurement = new Measurement(entityId, value, timestamp, isValid);
            return true;
        }

        private bool TryReadIntPart(string part, string key, out int value)
        {
            value = 0;
            string rawValue = ReadValueAfterEquals(part, key);

            return int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private bool TryReadDoublePart(string part, string key, out double value)
        {
            value = 0;
            string rawValue = ReadValueAfterEquals(part, key);

            return double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private bool TryReadStatusPart(string part, out bool isValid)
        {
            isValid = false;
            string status = ReadValueAfterEquals(part, "Status");

            if (status == "VALID")
            {
                isValid = true;
                return true;
            }

            if (status == "INVALID")
            {
                isValid = false;
                return true;
            }

            return false;
        }

        private string ReadValueAfterEquals(string part, string expectedKey)
        {
            if (string.IsNullOrWhiteSpace(part))
            {
                return string.Empty;
            }

            string[] keyValue = part.Trim().Split(new[] { '=' }, 2);

            if (keyValue.Length != 2)
            {
                return string.Empty;
            }

            if (keyValue[0].Trim() != expectedKey)
            {
                return string.Empty;
            }

            return keyValue[1].Trim();
        }
    }
}