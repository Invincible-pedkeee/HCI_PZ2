using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using NetworkService.Helpers;
using NetworkService.Helpers.Graph;
using NetworkService.Model;
using NetworkService.Services;

namespace NetworkService.ViewModel
{
    public class MeasurementGraphViewModel : BindableBase
    {
        private readonly NetworkDataService dataService;
        private readonly MeasurementGraphBuilderService graphBuilderService;

        private NetworkEntity selectedEntity;
        private ObservableCollection<Measurement> lastMeasurements;
        private ObservableCollection<MeasurementGraphLine> measurementLines;
        private ObservableCollection<MeasurementGraphMarker> measurementMarkers;
        private ObservableCollection<MeasurementGraphLabel> measurementLabels;
        private ObservableCollection<DistributionSegment> distributionSegments;

        private bool hasMeasurements;
        private string minValueLabel;
        private string maxValueLabel;
        private double iaPercentage;
        private double ibPercentage;
        private string distributionText;

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

        public ObservableCollection<MeasurementGraphLine> MeasurementLines
        {
            get
            {
                return measurementLines;
            }
            set
            {
                SetProperty(ref measurementLines, value);
            }
        }

        public ObservableCollection<MeasurementGraphMarker> MeasurementMarkers
        {
            get
            {
                return measurementMarkers;
            }
            set
            {
                SetProperty(ref measurementMarkers, value);
            }
        }

        public ObservableCollection<MeasurementGraphLabel> MeasurementLabels
        {
            get
            {
                return measurementLabels;
            }
            set
            {
                SetProperty(ref measurementLabels, value);
            }
        }

        public ObservableCollection<DistributionSegment> DistributionSegments
        {
            get
            {
                return distributionSegments;
            }
            set
            {
                SetProperty(ref distributionSegments, value);
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

        public bool HasMeasurements
        {
            get
            {
                return hasMeasurements;
            }
            set
            {
                SetProperty(ref hasMeasurements, value);
                OnPropertyChanged("HasNoMeasurements");
            }
        }

        public bool HasNoMeasurements
        {
            get
            {
                return !HasMeasurements;
            }
        }

        public string MinValueLabel
        {
            get
            {
                return minValueLabel;
            }
            set
            {
                SetProperty(ref minValueLabel, value);
            }
        }

        public string MaxValueLabel
        {
            get
            {
                return maxValueLabel;
            }
            set
            {
                SetProperty(ref maxValueLabel, value);
            }
        }

        public double MeasurementCanvasWidth
        {
            get
            {
                return graphBuilderService.MeasurementCanvasWidth;
            }
        }

        public double MeasurementCanvasHeight
        {
            get
            {
                return graphBuilderService.MeasurementCanvasHeight;
            }
        }

        public double DistributionCanvasWidth
        {
            get
            {
                return graphBuilderService.DistributionCanvasWidth;
            }
        }

        public double DistributionCanvasHeight
        {
            get
            {
                return graphBuilderService.DistributionCanvasHeight;
            }
        }

        public double IaPercentage
        {
            get
            {
                return iaPercentage;
            }
            set
            {
                SetProperty(ref iaPercentage, value);
            }
        }

        public double IbPercentage
        {
            get
            {
                return ibPercentage;
            }
            set
            {
                SetProperty(ref ibPercentage, value);
            }
        }

        public string DistributionText
        {
            get
            {
                return distributionText;
            }
            set
            {
                SetProperty(ref distributionText, value);
            }
        }

        public MeasurementGraphViewModel(NetworkDataService dataService)
        {
            this.dataService = dataService;
            graphBuilderService = new MeasurementGraphBuilderService();

            LastMeasurements = new ObservableCollection<Measurement>();
            MeasurementLines = new ObservableCollection<MeasurementGraphLine>();
            MeasurementMarkers = new ObservableCollection<MeasurementGraphMarker>();
            MeasurementLabels = new ObservableCollection<MeasurementGraphLabel>();
            DistributionSegments = new ObservableCollection<DistributionSegment>();

            MinValueLabel = string.Empty;
            MaxValueLabel = string.Empty;
            DistributionText = "Tip IA: 0.0% | Tip IB: 0.0%";

            dataService.Measurements.CollectionChanged += Measurements_CollectionChanged;
            dataService.Entities.CollectionChanged += Entities_CollectionChanged;

            if (dataService.Entities.Count > 0)
            {
                SelectedEntity = dataService.Entities[0];
            }

            RefreshLastMeasurements();
            RefreshDistributionGraph();
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

            RefreshDistributionGraph();
        }

        private void RefreshLastMeasurements()
        {
            if (SelectedEntity == null)
            {
                LastMeasurements = new ObservableCollection<Measurement>();

                ApplyMeasurementGraphResult(
                    graphBuilderService.BuildMeasurementGraph(LastMeasurements));

                return;
            }

            LastMeasurements = dataService.GetLastMeasurementsForEntity(SelectedEntity.Id, 5);

            ApplyMeasurementGraphResult(
                graphBuilderService.BuildMeasurementGraph(LastMeasurements));
        }

        private void RefreshDistributionGraph()
        {
            DistributionGraphBuildResult result =
                graphBuilderService.BuildDistributionGraph(dataService.Entities);

            DistributionSegments = result.Segments;
            IaPercentage = result.IaPercentage;
            IbPercentage = result.IbPercentage;
            DistributionText = result.DistributionText;
        }

        private void ApplyMeasurementGraphResult(MeasurementGraphBuildResult result)
        {
            MeasurementLines = result.Lines;
            MeasurementMarkers = result.Markers;
            MeasurementLabels = result.Labels;

            MinValueLabel = result.MinValueLabel;
            MaxValueLabel = result.MaxValueLabel;
            HasMeasurements = result.HasMeasurements;
        }
    }
}