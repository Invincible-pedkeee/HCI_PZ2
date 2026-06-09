using NetworkService.Helpers;

namespace NetworkService.Model
{
    public class NetworkEntity : BindableBase
    {
        private int id;
        private string name;
        private EntityType type;
        private double lastValue;
        private bool isPlacedOnDisplay;

        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                SetProperty(ref id, value);
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                SetProperty(ref name, value);
            }
        }

        public EntityType Type
        {
            get
            {
                return type;
            }
            set
            {
                SetProperty(ref type, value);
                OnPropertyChanged("Status");
                OnPropertyChanged("IsValueValid");
            }
        }

        public double LastValue
        {
            get
            {
                return lastValue;
            }
            set
            {
                SetProperty(ref lastValue, value);
                OnPropertyChanged("Status");
                OnPropertyChanged("IsValueValid");
            }
        }

        public bool IsPlacedOnDisplay
        {
            get
            {
                return isPlacedOnDisplay;
            }
            set
            {
                SetProperty(ref isPlacedOnDisplay, value);
            }
        }

        public bool IsValueValid
        {
            get
            {
                if (Type == null)
                {
                    return true;
                }

                return LastValue <= Type.MaxAllowedValue;
            }
        }

        public string Status
        {
            get
            {
                if (IsValueValid)
                {
                    return "OK";
                }

                return "PREKORACENJE";
            }
        }

        public NetworkEntity()
        {
        }

        public NetworkEntity(int id, string name, EntityType type, double lastValue)
        {
            Id = id;
            Name = name;
            Type = type;
            LastValue = lastValue;
            IsPlacedOnDisplay = false;
        }
    }
}