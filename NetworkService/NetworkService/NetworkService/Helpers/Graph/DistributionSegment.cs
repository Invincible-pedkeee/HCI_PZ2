namespace NetworkService.Helpers.Graph
{
    public class DistributionSegment
    {
        public string TypeName { get; set; }
        public double Left { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Text { get; set; }

        public DistributionSegment(string typeName, double left, double width, double height, string text)
        {
            TypeName = typeName;
            Left = left;
            Width = width;
            Height = height;
            Text = text;
        }
    }
}