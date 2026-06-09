namespace NetworkService.Model
{
    public class EntityType
    {
        public string Name { get; set; }

        public string ImagePath { get; set; }

        public double MaxAllowedValue { get; set; }

        public EntityType()
        {
        }

        public EntityType(string name, string imagePath, double maxAllowedValue)
        {
            Name = name;
            ImagePath = imagePath;
            MaxAllowedValue = maxAllowedValue;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}