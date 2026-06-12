using System.Collections.ObjectModel;

namespace NetworkService.Helpers.Graph
{
    public class MeasurementGraphBuildResult
    {
        public ObservableCollection<MeasurementGraphLine> Lines { get; private set; }
        public ObservableCollection<MeasurementGraphMarker> Markers { get; private set; }
        public ObservableCollection<MeasurementGraphLabel> Labels { get; private set; }

        public bool HasMeasurements { get; set; }
        public string MinValueLabel { get; set; }
        public string MaxValueLabel { get; set; }

        public MeasurementGraphBuildResult()
        {
            Lines = new ObservableCollection<MeasurementGraphLine>();
            Markers = new ObservableCollection<MeasurementGraphMarker>();
            Labels = new ObservableCollection<MeasurementGraphLabel>();

            HasMeasurements = false;
            MinValueLabel = string.Empty;
            MaxValueLabel = string.Empty;
        }
    }
}