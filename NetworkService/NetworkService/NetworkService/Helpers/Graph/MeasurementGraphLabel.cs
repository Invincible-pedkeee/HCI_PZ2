namespace NetworkService.Helpers.Graph
{
    public class MeasurementGraphLabel
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public string Text { get; set; }

        public MeasurementGraphLabel(double left, double top, string text)
        {
            Left = left;
            Top = top;
            Text = text;
        }
    }
}