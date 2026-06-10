using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using NetworkService.Model;
using NetworkService.ViewModel;

namespace NetworkService.Views
{
    public partial class MeasurementGraphView : UserControl
    {
        private MeasurementGraphViewModel subscribedViewModel;

        public MeasurementGraphView()
        {
            InitializeComponent();

            Loaded += MeasurementGraphView_Loaded;
            DataContextChanged += MeasurementGraphView_DataContextChanged;
        }

        private void MeasurementGraphView_Loaded(object sender, RoutedEventArgs e)
        {
            SubscribeToViewModel();
            RedrawAllGraphs();
        }

        private void MeasurementGraphView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UnsubscribeFromViewModel();
            SubscribeToViewModel();
            RedrawAllGraphs();
        }

        private void SubscribeToViewModel()
        {
            MeasurementGraphViewModel viewModel = DataContext as MeasurementGraphViewModel;

            if (viewModel == null || subscribedViewModel == viewModel)
            {
                return;
            }

            subscribedViewModel = viewModel;
            subscribedViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void UnsubscribeFromViewModel()
        {
            if (subscribedViewModel == null)
            {
                return;
            }

            subscribedViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            subscribedViewModel = null;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LastMeasurements" ||
                e.PropertyName == "SelectedEntity")
            {
                RedrawMeasurementGraph();
            }

            if (e.PropertyName == "IaPercentage" ||
                e.PropertyName == "IbPercentage" ||
                e.PropertyName == "DistributionText")
            {
                RedrawDistributionGraph();
            }
        }

        private void MeasurementCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawMeasurementGraph();
        }

        private void DistributionCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawDistributionGraph();
        }

        private void RedrawAllGraphs()
        {
            RedrawMeasurementGraph();
            RedrawDistributionGraph();
        }

        private void RedrawMeasurementGraph()
        {
            if (MeasurementCanvas == null)
            {
                return;
            }

            MeasurementCanvas.Children.Clear();

            MeasurementGraphViewModel viewModel = DataContext as MeasurementGraphViewModel;

            if (viewModel == null ||
                viewModel.SelectedEntity == null ||
                viewModel.LastMeasurements == null ||
                viewModel.LastMeasurements.Count == 0)
            {
                DrawCenteredText(MeasurementCanvas, "Nema mjerenja za izabrani entitet.");
                return;
            }

            double width = MeasurementCanvas.ActualWidth;
            double height = MeasurementCanvas.ActualHeight;

            if (width <= 0 || height <= 0)
            {
                return;
            }

            double leftMargin = 60;
            double rightMargin = 40;
            double topMargin = 35;
            double bottomMargin = 48;

            DrawAxes(width, height, leftMargin, rightMargin, topMargin, bottomMargin);

            Measurement[] measurements = viewModel.LastMeasurements.ToArray();

            double minValue = measurements.Min(measurement => measurement.Value);
            double maxValue = measurements.Max(measurement => measurement.Value);

            if (Math.Abs(maxValue - minValue) < 0.001)
            {
                maxValue = minValue + 1;
            }

            Point[] points = CalculateGraphPoints(
                measurements,
                width,
                height,
                leftMargin,
                rightMargin,
                topMargin,
                bottomMargin,
                minValue,
                maxValue);

            DrawConnectingLines(points);
            DrawMarkers(measurements, points);
            DrawTimeLabels(measurements, points, height, bottomMargin);
            DrawValueRangeLabels(minValue, maxValue, leftMargin, topMargin, height, bottomMargin);
        }

        private void DrawAxes(
            double width,
            double height,
            double leftMargin,
            double rightMargin,
            double topMargin,
            double bottomMargin)
        {
            Line yAxis = new Line
            {
                X1 = leftMargin,
                Y1 = topMargin,
                X2 = leftMargin,
                Y2 = height - bottomMargin,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };

            Line xAxis = new Line
            {
                X1 = leftMargin,
                Y1 = height - bottomMargin,
                X2 = width - rightMargin,
                Y2 = height - bottomMargin,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };

            MeasurementCanvas.Children.Add(yAxis);
            MeasurementCanvas.Children.Add(xAxis);

            TextBlock valueLabel = new TextBlock
            {
                Text = "Vrijednost",
                FontSize = 11
            };

            Canvas.SetLeft(valueLabel, 12);
            Canvas.SetTop(valueLabel, 8);
            MeasurementCanvas.Children.Add(valueLabel);

            TextBlock timeLabel = new TextBlock
            {
                Text = "Vrijeme",
                FontSize = 11
            };

            Canvas.SetLeft(timeLabel, width - 80);
            Canvas.SetTop(timeLabel, height - bottomMargin + 24);
            MeasurementCanvas.Children.Add(timeLabel);
        }

        private Point[] CalculateGraphPoints(
            Measurement[] measurements,
            double width,
            double height,
            double leftMargin,
            double rightMargin,
            double topMargin,
            double bottomMargin,
            double minValue,
            double maxValue)
        {
            Point[] points = new Point[measurements.Length];

            for (int i = 0; i < measurements.Length; i++)
            {
                double x;

                if (measurements.Length == 1)
                {
                    x = width / 2;
                }
                else
                {
                    x = leftMargin + i * ((width - leftMargin - rightMargin) / (measurements.Length - 1));
                }

                double normalizedValue = (measurements[i].Value - minValue) / (maxValue - minValue);
                double y = height - bottomMargin - normalizedValue * (height - topMargin - bottomMargin);

                points[i] = new Point(x, y);
            }

            return points;
        }

        private void DrawConnectingLines(Point[] points)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                Line line = new Line
                {
                    X1 = points[i].X,
                    Y1 = points[i].Y,
                    X2 = points[i + 1].X,
                    Y2 = points[i + 1].Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.5
                };

                MeasurementCanvas.Children.Add(line);
            }
        }

        private void DrawMarkers(Measurement[] measurements, Point[] points)
        {
            const double markerSize = 46;

            for (int i = 0; i < measurements.Length; i++)
            {
                Brush fillBrush = measurements[i].IsValid
                    ? Brushes.White
                    : new SolidColorBrush(Color.FromRgb(255, 179, 179));

                Ellipse marker = new Ellipse
                {
                    Width = markerSize,
                    Height = markerSize,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.5,
                    Fill = fillBrush
                };

                Canvas.SetLeft(marker, points[i].X - markerSize / 2);
                Canvas.SetTop(marker, points[i].Y - markerSize / 2);
                MeasurementCanvas.Children.Add(marker);

                TextBlock valueText = new TextBlock
                {
                    Text = measurements[i].Value.ToString("0"),
                    FontSize = 11,
                    Width = markerSize,
                    TextAlignment = TextAlignment.Center
                };

                Canvas.SetLeft(valueText, points[i].X - markerSize / 2);
                Canvas.SetTop(valueText, points[i].Y - 8);
                MeasurementCanvas.Children.Add(valueText);

                if (!measurements[i].IsValid)
                {
                    TextBlock warningText = new TextBlock
                    {
                        Text = "!",
                        FontSize = 12,
                        FontWeight = FontWeights.Bold,
                        Width = markerSize,
                        TextAlignment = TextAlignment.Center
                    };

                    Canvas.SetLeft(warningText, points[i].X - markerSize / 2);
                    Canvas.SetTop(warningText, points[i].Y + 8);
                    MeasurementCanvas.Children.Add(warningText);
                }
            }
        }

        private void DrawTimeLabels(Measurement[] measurements, Point[] points, double height, double bottomMargin)
        {
            for (int i = 0; i < measurements.Length; i++)
            {
                TextBlock timeText = new TextBlock
                {
                    Text = measurements[i].Timestamp.ToString("HH:mm:ss"),
                    FontSize = 10,
                    Width = 70,
                    TextAlignment = TextAlignment.Center
                };

                Canvas.SetLeft(timeText, points[i].X - 35);
                Canvas.SetTop(timeText, height - bottomMargin + 8);
                MeasurementCanvas.Children.Add(timeText);
            }
        }

        private void DrawValueRangeLabels(
            double minValue,
            double maxValue,
            double leftMargin,
            double topMargin,
            double height,
            double bottomMargin)
        {
            TextBlock maxText = new TextBlock
            {
                Text = maxValue.ToString("0"),
                FontSize = 10,
                Foreground = Brushes.Gray
            };

            Canvas.SetLeft(maxText, 20);
            Canvas.SetTop(maxText, topMargin - 8);
            MeasurementCanvas.Children.Add(maxText);

            TextBlock minText = new TextBlock
            {
                Text = minValue.ToString("0"),
                FontSize = 10,
                Foreground = Brushes.Gray
            };

            Canvas.SetLeft(minText, 20);
            Canvas.SetTop(minText, height - bottomMargin - 8);
            MeasurementCanvas.Children.Add(minText);
        }

        private void RedrawDistributionGraph()
        {
            if (DistributionCanvas == null)
            {
                return;
            }

            DistributionCanvas.Children.Clear();

            MeasurementGraphViewModel viewModel = DataContext as MeasurementGraphViewModel;

            if (viewModel == null)
            {
                DrawCenteredText(DistributionCanvas, "Nema podataka o tipovima.");
                return;
            }

            double width = DistributionCanvas.ActualWidth;
            double height = DistributionCanvas.ActualHeight;

            if (width <= 0 || height <= 0)
            {
                return;
            }

            double iaWidth = width * viewModel.IaPercentage / 100.0;
            double ibWidth = width - iaWidth;

            Rectangle iaRectangle = new Rectangle
            {
                Width = iaWidth,
                Height = height,
                Fill = Brushes.LightSteelBlue,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };

            Canvas.SetLeft(iaRectangle, 0);
            Canvas.SetTop(iaRectangle, 0);
            DistributionCanvas.Children.Add(iaRectangle);

            Rectangle ibRectangle = new Rectangle
            {
                Width = ibWidth,
                Height = height,
                Fill = Brushes.LightCyan,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };

            Canvas.SetLeft(ibRectangle, iaWidth);
            Canvas.SetTop(ibRectangle, 0);
            DistributionCanvas.Children.Add(ibRectangle);

            AddDistributionText(
                "IA " + viewModel.IaPercentage.ToString("0.0") + "%",
                iaWidth / 2 - 40,
                height / 2 - 8);

            AddDistributionText(
                "IB " + viewModel.IbPercentage.ToString("0.0") + "%",
                iaWidth + ibWidth / 2 - 40,
                height / 2 - 8);
        }

        private void AddDistributionText(string text, double x, double y)
        {
            TextBlock textBlock = new TextBlock
            {
                Text = text,
                FontSize = 11,
                Width = 80,
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.SemiBold
            };

            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            DistributionCanvas.Children.Add(textBlock);
        }

        private void DrawCenteredText(Canvas canvas, string text)
        {
            canvas.Children.Clear();

            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;

            TextBlock textBlock = new TextBlock
            {
                Text = text,
                FontSize = 13,
                Foreground = Brushes.Gray,
                Width = 260,
                TextAlignment = TextAlignment.Center
            };

            Canvas.SetLeft(textBlock, width / 2 - 130);
            Canvas.SetTop(textBlock, height / 2 - 10);
            canvas.Children.Add(textBlock);
        }
    }
}