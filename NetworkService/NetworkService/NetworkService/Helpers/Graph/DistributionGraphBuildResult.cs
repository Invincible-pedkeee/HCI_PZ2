using System.Collections.ObjectModel;

namespace NetworkService.Helpers.Graph
{
    public class DistributionGraphBuildResult
    {
        public ObservableCollection<DistributionSegment> Segments { get; private set; }

        public double IaPercentage { get; set; }
        public double IbPercentage { get; set; }
        public string DistributionText { get; set; }

        public DistributionGraphBuildResult()
        {
            Segments = new ObservableCollection<DistributionSegment>();
            IaPercentage = 0;
            IbPercentage = 0;
            DistributionText = "Tip IA: 0.0% | Tip IB: 0.0%";
        }
    }
}