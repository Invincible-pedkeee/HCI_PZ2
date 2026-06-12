using System;
using System.Collections.ObjectModel;
using System.Linq;
using NetworkService.Helpers.Graph;
using NetworkService.Model;

namespace NetworkService.Services
{
    public class MeasurementGraphBuilderService
    {
        private const double MeasurementCanvasWidthValue = 900;
        private const double MeasurementCanvasHeightValue = 280;
        private const double DistributionCanvasWidthValue = 900;
        private const double DistributionCanvasHeightValue = 55;

        private const double LeftMargin = 60;
        private const double RightMargin = 40;
        private const double TopMargin = 35;
        private const double BottomMargin = 48;
        private const double MarkerSize = 46;

        public double MeasurementCanvasWidth
        {
            get
            {
                return MeasurementCanvasWidthValue;
            }
        }

        public double MeasurementCanvasHeight
        {
            get
            {
                return MeasurementCanvasHeightValue;
            }
        }

        public double DistributionCanvasWidth
        {
            get
            {
                return DistributionCanvasWidthValue;
            }
        }

        public double DistributionCanvasHeight
        {
            get
            {
                return DistributionCanvasHeightValue;
            }
        }

        public MeasurementGraphBuildResult BuildMeasurementGraph(ObservableCollection<Measurement> measurements)
        {
            MeasurementGraphBuildResult result = new MeasurementGraphBuildResult();

            if (measurements == null || measurements.Count == 0)
            {
                return result;
            }

            Measurement[] measurementArray = measurements.ToArray();

            double minValue = measurementArray.Min(measurement => measurement.Value);
            double maxValue = measurementArray.Max(measurement => measurement.Value);

            if (Math.Abs(maxValue - minValue) < 0.001)
            {
                maxValue = minValue + 1;
            }

            MeasurementGraphMarker[] markers = CalculateMarkers(measurementArray, minValue, maxValue);

            for (int i = 0; i < markers.Length; i++)
            {
                result.Markers.Add(markers[i]);

                result.Labels.Add(new MeasurementGraphLabel(
                    markers[i].TimeLabelLeft,
                    MeasurementCanvasHeightValue - BottomMargin + 8,
                    measurementArray[i].Timestamp.ToString("HH:mm:ss")));
            }

            for (int i = 0; i < markers.Length - 1; i++)
            {
                result.Lines.Add(new MeasurementGraphLine(
                    markers[i].CenterX,
                    markers[i].CenterY,
                    markers[i + 1].CenterX,
                    markers[i + 1].CenterY));
            }

            result.MinValueLabel = minValue.ToString("0");
            result.MaxValueLabel = maxValue.ToString("0");
            result.HasMeasurements = true;

            return result;
        }

        public DistributionGraphBuildResult BuildDistributionGraph(ObservableCollection<NetworkEntity> entities)
        {
            DistributionGraphBuildResult result = new DistributionGraphBuildResult();

            double iaPercentage = CalculateTypePercentage(entities, "IA");
            double ibPercentage = CalculateTypePercentage(entities, "IB");

            double iaWidth = DistributionCanvasWidthValue * iaPercentage / 100.0;
            double ibWidth = DistributionCanvasWidthValue - iaWidth;

            result.Segments.Add(new DistributionSegment(
                "IA",
                0,
                iaWidth,
                DistributionCanvasHeightValue,
                "IA " + iaPercentage.ToString("0.0") + "%"));

            result.Segments.Add(new DistributionSegment(
                "IB",
                iaWidth,
                ibWidth,
                DistributionCanvasHeightValue,
                "IB " + ibPercentage.ToString("0.0") + "%"));

            result.IaPercentage = iaPercentage;
            result.IbPercentage = ibPercentage;

            result.DistributionText =
                "Tip IA: " + iaPercentage.ToString("0.0") +
                "% | Tip IB: " + ibPercentage.ToString("0.0") + "%";

            return result;
        }

        private MeasurementGraphMarker[] CalculateMarkers(
            Measurement[] measurements,
            double minValue,
            double maxValue)
        {
            MeasurementGraphMarker[] markers = new MeasurementGraphMarker[measurements.Length];

            for (int i = 0; i < measurements.Length; i++)
            {
                double x;

                if (measurements.Length == 1)
                {
                    x = MeasurementCanvasWidthValue / 2;
                }
                else
                {
                    x = LeftMargin + i *
                        ((MeasurementCanvasWidthValue - LeftMargin - RightMargin) /
                        (measurements.Length - 1));
                }

                double normalizedValue =
                    (measurements[i].Value - minValue) / (maxValue - minValue);

                double y =
                    MeasurementCanvasHeightValue -
                    BottomMargin -
                    normalizedValue *
                    (MeasurementCanvasHeightValue - TopMargin - BottomMargin);

                markers[i] = new MeasurementGraphMarker(
                    x,
                    y,
                    x - MarkerSize / 2,
                    y - MarkerSize / 2,
                    MarkerSize,
                    measurements[i].Value.ToString("0"),
                    x - 35,
                    !measurements[i].IsValid);
            }

            return markers;
        }

        private double CalculateTypePercentage(
            ObservableCollection<NetworkEntity> entities,
            string typeName)
        {
            if (entities == null || entities.Count == 0)
            {
                return 0;
            }

            int typeCount = entities.Count(entity =>
                entity.Type != null &&
                entity.Type.Name == typeName);

            return typeCount * 100.0 / entities.Count;
        }
    }
}