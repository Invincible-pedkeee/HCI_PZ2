using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using NetworkService.Helpers;
using NetworkService.Model;
using NetworkService.Services;

namespace NetworkService.ViewModel
{
    public class MeasurementGraphViewModel : BindableBase
    {
        private readonly NetworkDataService dataService;

        private NetworkEntity selectedEntity;
        private ObservableCollection<Measurement> lastMeasurements;

        public ObservableCollection<NetworkEntity> Entities
        {
            get
            {
                return dataService.Entities;
            }
        }

        public ObservableCollection<Measurement> LastMeasurements
        {
            get
            {
                return lastMeasurements;
            }
            set
            {
                SetProperty(ref lastMeasurements, value);
            }
        }

        public NetworkEntity SelectedEntity
        {
            get
            {
                return selectedEntity;
            }
            set
            {
                SetProperty(ref selectedEntity, value);
                RefreshLastMeasurements();
            }
        }

        public double IaPercentage
        {
            get
            {
                return CalculateTypePercentage("IA");
            }
        }

        public double IbPercentage
        {
            get
            {
                return CalculateTypePercentage("IB");
            }
        }

        public string DistributionText
        {
            get
            {
                return "Tip IA: " + IaPercentage.ToString("0.0") +
                       "% | Tip IB: " + IbPercentage.ToString("0.0") + "%";
            }
        }

        public MeasurementGraphViewModel(NetworkDataService dataService)
        {
            this.dataService = dataService;

            LastMeasurements = new ObservableCollection<Measurement>();

            dataService.Measurements.CollectionChanged += Measurements_CollectionChanged;
            dataService.Entities.CollectionChanged += Entities_CollectionChanged;

            if (dataService.Entities.Count > 0)
            {
                SelectedEntity = dataService.Entities[0];
            }

            RefreshLastMeasurements();
        }

        private void Measurements_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshLastMeasurements();
        }

        private void Entities_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SelectedEntity != null && !dataService.Entities.Contains(SelectedEntity))
            {
                if (dataService.Entities.Count > 0)
                {
                    SelectedEntity = dataService.Entities[0];
                }
                else
                {
                    SelectedEntity = null;
                }
            }

            if (SelectedEntity == null && dataService.Entities.Count > 0)
            {
                SelectedEntity = dataService.Entities[0];
            }

            OnPropertyChanged("IaPercentage");
            OnPropertyChanged("IbPercentage");
            OnPropertyChanged("DistributionText");
        }

        private void RefreshLastMeasurements()
        {
            if (SelectedEntity == null)
            {
                LastMeasurements = new ObservableCollection<Measurement>();
                return;
            }

            LastMeasurements = dataService.GetLastMeasurementsForEntity(SelectedEntity.Id, 5);
        }

        private double CalculateTypePercentage(string typeName)
        {
            if (dataService.Entities.Count == 0)
            {
                return 0;
            }

            int typeCount = dataService.Entities.Count(entity =>
                entity.Type != null &&
                entity.Type.Name == typeName);

            return typeCount * 100.0 / dataService.Entities.Count;
        }
    }
}