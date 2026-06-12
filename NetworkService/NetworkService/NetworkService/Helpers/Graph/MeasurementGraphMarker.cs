namespace NetworkService.Helpers.Graph
{
    public class MeasurementGraphMarker
    {
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Size { get; set; }
        public string ValueText { get; set; }
        public double TimeLabelLeft { get; set; }
        public bool IsInvalid { get; set; }

        public MeasurementGraphMarker(
            double centerX,
            double centerY,
            double left,
            double top,
            double size,
            string valueText,
            double timeLabelLeft,
            bool isInvalid)
        {
            CenterX = centerX;
            CenterY = centerY;
            Left = left;
            Top = top;
            Size = size;
            ValueText = valueText;
            TimeLabelLeft = timeLabelLeft;
            IsInvalid = isInvalid;
        }
    }
}