using NetworkService.Helpers;
using NetworkService.Services;

namespace NetworkService.ViewModel
{
    public class NetworkDisplayViewModel : BindableBase
    {
        private readonly NetworkDataService dataService;

        public NetworkDisplayViewModel(NetworkDataService dataService)
        {
            this.dataService = dataService;
        }
    }
}