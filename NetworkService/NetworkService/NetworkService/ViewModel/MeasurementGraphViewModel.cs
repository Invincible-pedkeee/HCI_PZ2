using NetworkService.Helpers;
using NetworkService.Services;

namespace NetworkService.ViewModel
{
    public class MeasurementGraphViewModel : BindableBase
    {
        private readonly NetworkDataService dataService;

        public MeasurementGraphViewModel(NetworkDataService dataService)
        {
            this.dataService = dataService;
        }
    }
}