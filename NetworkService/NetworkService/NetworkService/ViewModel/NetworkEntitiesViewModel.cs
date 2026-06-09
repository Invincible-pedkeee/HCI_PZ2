using NetworkService.Helpers;
using NetworkService.Services;

namespace NetworkService.ViewModel
{
    public class NetworkEntitiesViewModel : BindableBase
    {
        private readonly NetworkDataService dataService;

        public NetworkEntitiesViewModel(NetworkDataService dataService)
        {
            this.dataService = dataService;
        }
    }
}