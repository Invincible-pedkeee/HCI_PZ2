using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetworkService.Services
{
    public class MeteringReceiverService
    {
        private const int Port = 25675;

        private readonly Func<int> getEntityCount;
        private readonly Action<int, double> measurementReceived;
        private readonly Action<string> statusChanged;

        private TcpListener tcpListener;
        private Thread listenerThread;
        private bool isRunning;

        public MeteringReceiverService(
            Func<int> getEntityCount,
            Action<int, double> measurementReceived,
            Action<string> statusChanged)
        {
            this.getEntityCount = getEntityCount;
            this.measurementReceived = measurementReceived;
            this.statusChanged = statusChanged;
        }

        public void Start()
        {
            if (isRunning)
            {
                return;
            }

            isRunning = true;

            listenerThread = new Thread(ListenForMessages);
            listenerThread.IsBackground = true;
            listenerThread.Start();
        }

        public void Stop()
        {
            isRunning = false;

            if (tcpListener != null)
            {
                tcpListener.Stop();
            }
        }

        private void ListenForMessages()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, Port);
                tcpListener.Start();

                NotifyStatus("Metering listener started on port " + Port + ".");

                while (isRunning)
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    HandleClient(client);
                }
            }
            catch (SocketException)
            {
                NotifyStatus("Metering listener stopped.");
            }
            catch (Exception ex)
            {
                NotifyStatus("Metering listener error: " + ex.Message);
            }
        }

        private void HandleClient(TcpClient client)
        {
            using (client)
            {
                NetworkStream stream = client.GetStream();

                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead <= 0)
                {
                    return;
                }

                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                if (IsCountRequest(message))
                {
                    SendEntityCount(stream);
                    return;
                }

                ProcessMeasurementMessage(message);
            }
        }

        private bool IsCountRequest(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            string normalizedMessage = message.ToLowerInvariant();

            return normalizedMessage.Contains("count") ||
                   normalizedMessage.Contains("broj") ||
                   normalizedMessage.Contains("koliko");
        }

        private void SendEntityCount(NetworkStream stream)
        {
            int count = getEntityCount();

            byte[] response = Encoding.ASCII.GetBytes(count.ToString(CultureInfo.InvariantCulture));
            stream.Write(response, 0, response.Length);

            NotifyStatus("Simulator requested entity count. Sent count: " + count + ".");
        }

        private void ProcessMeasurementMessage(string message)
        {
            int simulatorIndex;
            double value;

            if (!TryParseMeasurementMessage(message, out simulatorIndex, out value))
            {
                NotifyStatus("Invalid simulator message received: " + message);
                return;
            }

            measurementReceived(simulatorIndex, value);
        }

        private bool TryParseMeasurementMessage(string message, out int simulatorIndex, out double value)
        {
            simulatorIndex = -1;
            value = 0;

            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            string[] mainParts = message.Split(':');

            if (mainParts.Length != 2)
            {
                return false;
            }

            string entityPart = mainParts[0].Trim();
            string valuePart = mainParts[1].Trim();

            if (!entityPart.StartsWith("Entitet_"))
            {
                return false;
            }

            string indexText = entityPart.Replace("Entitet_", string.Empty);

            if (!int.TryParse(indexText, out simulatorIndex))
            {
                return false;
            }

            if (!double.TryParse(valuePart, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                return false;
            }

            return true;
        }

        private void NotifyStatus(string message)
        {
            if (statusChanged != null)
            {
                statusChanged(message);
            }
        }
    }
}