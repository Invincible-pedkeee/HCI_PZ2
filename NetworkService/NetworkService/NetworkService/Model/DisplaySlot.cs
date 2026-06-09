using System.ComponentModel;
using NetworkService.Helpers;

namespace NetworkService.Model
{
    public class DisplaySlot : BindableBase
    {
        private NetworkEntity occupiedEntity;

        public int SlotNumber { get; set; }

        public NetworkEntity OccupiedEntity
        {
            get
            {
                return occupiedEntity;
            }
            set
            {
                if (occupiedEntity == value)
                {
                    return;
                }

                if (occupiedEntity != null)
                {
                    occupiedEntity.PropertyChanged -= OccupiedEntityPropertyChanged;
                }

                occupiedEntity = value;

                if (occupiedEntity != null)
                {
                    occupiedEntity.PropertyChanged += OccupiedEntityPropertyChanged;
                }

                OnPropertyChanged();
                OnPropertyChanged("IsOccupied");
                OnPropertyChanged("TitleText");
                OnPropertyChanged("DetailsText");
                OnPropertyChanged("StatusText");
                OnPropertyChanged("IsValueInvalid");
            }
        }

        public bool IsOccupied
        {
            get
            {
                return OccupiedEntity != null;
            }
        }

        public bool IsValueInvalid
        {
            get
            {
                return IsOccupied && !OccupiedEntity.IsValueValid;
            }
        }

        public string TitleText
        {
            get
            {
                if (!IsOccupied)
                {
                    return "SLOT " + SlotNumber + " - PRAZNO";
                }

                return OccupiedEntity.Name;
            }
        }

        public string DetailsText
        {
            get
            {
                if (!IsOccupied)
                {
                    return "Prevuci entitet ovdje";
                }

                return "ID: " + OccupiedEntity.Id +
                       " | Tip: " + OccupiedEntity.Type.Name +
                       " | Vrijednost: " + OccupiedEntity.LastValue;
            }
        }

        public string StatusText
        {
            get
            {
                if (!IsOccupied)
                {
                    return "Status: slobodno polje";
                }

                return "Status: " + OccupiedEntity.Status;
            }
        }

        public DisplaySlot()
        {
        }

        public DisplaySlot(int slotNumber)
        {
            SlotNumber = slotNumber;
        }

        public void PlaceEntity(NetworkEntity entity)
        {
            OccupiedEntity = entity;

            if (entity != null)
            {
                entity.IsPlacedOnDisplay = true;
            }
        }

        public NetworkEntity RemoveEntity()
        {
            NetworkEntity removedEntity = OccupiedEntity;

            if (removedEntity != null)
            {
                removedEntity.IsPlacedOnDisplay = false;
            }

            OccupiedEntity = null;

            return removedEntity;
        }

        private void OccupiedEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("DetailsText");
            OnPropertyChanged("StatusText");
            OnPropertyChanged("IsValueInvalid");
        }
    }
}