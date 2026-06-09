using NetworkService.Helpers;
using NetworkService.Services;
using System.Windows;


namespace NetworkService.ViewModel
{
    public class MainWindowViewModel : BindableBase
    {
        private BindableBase currentViewModel;
        private string statusMessage;

        private readonly MeteringReceiverService meteringReceiverService;
        private readonly NetworkDataService dataService;

        private readonly NetworkEntitiesViewModel networkEntitiesViewModel;
        private readonly NetworkDisplayViewModel networkDisplayViewModel;
        private readonly MeasurementGraphViewModel measurementGraphViewModel;

        public MyICommandWithParameter<string> NavigationCommand { get; private set; }

        public BindableBase CurrentViewModel
        {
            get
            {
                return currentViewModel;
            }
            set
            {
                SetProperty(ref currentViewModel, value);
            }
        }
        private int GetEntityCount()
        {
            return dataService.Entities.Count;
        }

        private void OnMeasurementReceived(int simulatorIndex, double value)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                NetworkService.Model.NetworkEntity entity = dataService.GetEntityBySimulatorIndex(simulatorIndex);

                if (entity == null)
                {
                    string message = "Primljeno mjerenje za nepostojeći simulator indeks: " + simulatorIndex;
                    dataService.AddHistory(message);
                    StatusMessage = message;
                    return;
                }

                dataService.AddMeasurement(entity, value);

                StatusMessage =
                    "Primljeno mjerenje: ID " +
                    entity.Id +
                    " = " +
                    value +
                    " | Status: " +
                    entity.Status;
            });
        }

        private void OnMeteringStatusChanged(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusMessage = message;
            });
        }

        public void StopServices()
        {
            if (meteringReceiverService != null)
            {
                meteringReceiverService.Stop();
            }
        }
        public string StatusMessage
        {
            get
            {
                return statusMessage;
            }
            set
            {
                SetProperty(ref statusMessage, value);
            }
        }

        public MainWindowViewModel()
        {
            dataService = new NetworkDataService();

            networkEntitiesViewModel = new NetworkEntitiesViewModel(dataService);
            networkDisplayViewModel = new NetworkDisplayViewModel(dataService);
            measurementGraphViewModel = new MeasurementGraphViewModel(dataService);

            meteringReceiverService = new MeteringReceiverService(
            GetEntityCount,
            OnMeasurementReceived,
            OnMeteringStatusChanged);

            meteringReceiverService.Start();

            NavigationCommand = new MyICommandWithParameter<string>(OnNavigate);


            CurrentViewModel = networkEntitiesViewModel;
            StatusMessage = "Sistem spreman.";
        }

        private void OnNavigate(string destination)
        {
            if (destination == "entities")
            {
                CurrentViewModel = networkEntitiesViewModel;
                StatusMessage = "Prikazani entiteti mreže.";
            }
            else if (destination == "display")
            {
                networkDisplayViewModel.RefreshAvailableEntityGroups();
                CurrentViewModel = networkDisplayViewModel;
                StatusMessage = "Prikazana mreža za monitoring.";
            }
            else if (destination == "graph")
            {
                CurrentViewModel = measurementGraphViewModel;
                StatusMessage = "Prikazan grafikon mjerenja.";
            }
        }
    }
}