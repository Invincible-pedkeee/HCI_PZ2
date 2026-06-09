using NetworkService.Helpers;
using NetworkService.Services;

namespace NetworkService.ViewModel
{
    public class MainWindowViewModel : BindableBase
    {
        private BindableBase currentViewModel;
        private string statusMessage;

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