using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NetworkService.Services
{
    public class SimulatorRestartService
    {
        private const string SimulatorProcessName = "MeteringSimulator";

        private string simulatorExePath;

        public SimulatorRestartService()
        {
            simulatorExePath = FindSimulatorPath();
        }

        public void RestartSimulatorAsync()
        {
            Task.Run(() =>
            {
                RestartSimulator();
            });
        }

        private void RestartSimulator()
        {
            if (string.IsNullOrEmpty(simulatorExePath))
            {
                simulatorExePath = FindSimulatorPath();
            }

            StopSimulator();

            Task.Delay(1000).Wait();

            StartSimulator();
        }

        private void StopSimulator()
        {
            Process[] processes = Process.GetProcessesByName(SimulatorProcessName);

            foreach (Process process in processes)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                        process.WaitForExit(3000);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void StartSimulator()
        {
            if (string.IsNullOrEmpty(simulatorExePath) || !File.Exists(simulatorExePath))
            {
                simulatorExePath = FindSimulatorPath();
            }

            if (!string.IsNullOrEmpty(simulatorExePath) && File.Exists(simulatorExePath))
            {
                try
                {
                    Process.Start(simulatorExePath);
                }
                catch (Exception)
                {
                }
            }
        }

        private string FindSimulatorPath()
        {
            Process[] processes = Process.GetProcessesByName(SimulatorProcessName);

            if (processes.Length > 0)
            {
                try
                {
                    string path = processes[0].MainModule.FileName;

                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
                catch (Exception)
                {
                }
            }

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string[] relativePaths = new string[]
            {
                Path.Combine(baseDirectory, "..", "..", "..", "..", "..", "MeteringSimulator", "MeteringSimulator", "bin", "Debug", "MeteringSimulator.exe"),
                Path.Combine(baseDirectory, "..", "..", "..", "..", "MeteringSimulator", "MeteringSimulator", "bin", "Debug", "MeteringSimulator.exe"),
                Path.Combine(baseDirectory, "..", "..", "..", "MeteringSimulator", "bin", "Debug", "MeteringSimulator.exe"),
                Path.Combine(baseDirectory, "..", "..", "MeteringSimulator", "bin", "Debug", "MeteringSimulator.exe"),
                Path.Combine(baseDirectory, "MeteringSimulator.exe"),
            };

            foreach (string relativePath in relativePaths)
            {
                string fullPath = Path.GetFullPath(relativePath);

                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return string.Empty;
        }
    }
}