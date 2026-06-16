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

        // Malo manje tačke da se ne preklapaju sa vremenom i linijama
        private const double MarkerSize = 38;

        public double MeasurementCanvasWidth
        {
            get { return MeasurementCanvasWidthValue; }
        }

        public double MeasurementCanvasHeight
        {
            get { return MeasurementCanvasHeightValue; }
        }

        public double DistributionCanvasWidth
        {
            get { return DistributionCanvasWidthValue; }
        }

        public double DistributionCanvasHeight
        {
            get { return DistributionCanvasHeightValue; }
        }

        public MeasurementGraphBuildResult BuildMeasurementGraph(ObservableCollection<Measurement> measurements)
        {
            MeasurementGraphBuildResult result = new MeasurementGraphBuildResult();

            if (measurements == null || measurements.Count == 0)
            {
                return result;
            }

            Measurement[] measurementArray = measurements.ToArray();

            // Y osa uvijek kreće od 0
            double minValue = 0;

            // Gornja granica se zaokružuje na lijepu vrijednost, npr. 25000 umjesto 20672
            double maxValue = CalculateNiceAxisMaximum(measurementArray.Max(measurement => measurement.Value));

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

                if (normalizedValue < 0)
                {
                    normalizedValue = 0;
                }

                if (normalizedValue > 1)
                {
                    normalizedValue = 1;
                }

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
                    x - 39,
                    !measurements[i].IsValid);
            }

            return markers;
        }

        private double CalculateNiceAxisMaximum(double maxValue)
        {
            if (maxValue <= 0)
            {
                return 1;
            }

            double paddedValue = maxValue * 1.10;
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(paddedValue)));
            double normalizedValue = paddedValue / magnitude;

            double niceNormalizedValue;

            if (normalizedValue <= 1)
            {
                niceNormalizedValue = 1;
            }
            else if (normalizedValue <= 2)
            {
                niceNormalizedValue = 2;
            }
            else if (normalizedValue <= 2.5)
            {
                niceNormalizedValue = 2.5;
            }
            else if (normalizedValue <= 5)
            {
                niceNormalizedValue = 5;
            }
            else
            {
                niceNormalizedValue = 10;
            }

            return niceNormalizedValue * magnitude;
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