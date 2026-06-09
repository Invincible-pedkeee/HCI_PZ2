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



        public ToastNotificationService ToastService { get; private set; }
        public MyICommandWithParameter<string> NavigationCommand { get; private set; }


        public MyICommand GlobalAddCommand { get; private set; }

        public MyICommand GlobalDeleteCommand { get; private set; }

        public MyICommand GlobalUndoCommand { get; private set; }

        public MyICommand GlobalUndoAllCommand { get; private set; }

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
            ToastService = new ToastNotificationService();


            networkEntitiesViewModel = new NetworkEntitiesViewModel(dataService, ToastService);
            networkDisplayViewModel = new NetworkDisplayViewModel(dataService, ToastService);
            measurementGraphViewModel = new MeasurementGraphViewModel(dataService);


            NavigationCommand = new MyICommandWithParameter<string>(OnNavigate);
            GlobalAddCommand = new MyICommand(OnGlobalAdd);
            GlobalDeleteCommand = new MyICommand(OnGlobalDelete);
            GlobalUndoCommand = new MyICommand(OnGlobalUndo);
            GlobalUndoAllCommand = new MyICommand(OnGlobalUndoAll);

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
        private void OnGlobalAdd()
        {
            if (CurrentViewModel == networkEntitiesViewModel)
            {
                networkEntitiesViewModel.AddEntityCommand.Execute(null);
                return;
            }

            ToastService.ShowInfo("Dodavanje entiteta je dostupno samo na prikazu Entiteti mreže.");
        }

        private void OnGlobalDelete()
        {
            if (CurrentViewModel == networkEntitiesViewModel)
            {
                if (networkEntitiesViewModel.RequestDeleteEntityCommand.CanExecute(null))
                {
                    networkEntitiesViewModel.RequestDeleteEntityCommand.Execute(null);
                }
                else
                {
                    ToastService.ShowWarning("Prvo selektujte entitet koji želite da obrišete.");
                }

                return;
            }

            ToastService.ShowInfo("Brisanje selektovanog entiteta je dostupno samo na prikazu Entiteti mreže.");
        }

        private void OnGlobalUndo()
        {
            if (CurrentViewModel == networkEntitiesViewModel)
            {
                if (networkEntitiesViewModel.UndoCommand.CanExecute(null))
                {
                    networkEntitiesViewModel.UndoCommand.Execute(null);
                }
                else
                {
                    ToastService.ShowInfo("Nema akcija za poništavanje na prikazu Entiteti mreže.");
                }

                return;
            }

            if (CurrentViewModel == networkDisplayViewModel)
            {
                if (networkDisplayViewModel.UndoCommand.CanExecute(null))
                {
                    networkDisplayViewModel.UndoCommand.Execute(null);
                }
                else
                {
                    ToastService.ShowInfo("Nema akcija za poništavanje na prikazu mreže.");
                }

                return;
            }

            ToastService.ShowInfo("Undo nije dostupan na prikazu grafikona.");
        }

        private void OnGlobalUndoAll()
        {
            if (CurrentViewModel == networkEntitiesViewModel)
            {
                if (networkEntitiesViewModel.UndoAllCommand.CanExecute(null))
                {
                    networkEntitiesViewModel.UndoAllCommand.Execute(null);
                }
                else
                {
                    ToastService.ShowInfo("Nema akcija za poništavanje na prikazu Entiteti mreže.");
                }

                return;
            }

            if (CurrentViewModel == networkDisplayViewModel)
            {
                if (networkDisplayViewModel.UndoAllCommand.CanExecute(null))
                {
                    networkDisplayViewModel.UndoAllCommand.Execute(null);
                }
                else
                {
                    ToastService.ShowInfo("Nema akcija za poništavanje na prikazu mreže.");
                }

                return;
            }

            ToastService.ShowInfo("Undo All nije dostupan na prikazu grafikona.");
        }
    }
}