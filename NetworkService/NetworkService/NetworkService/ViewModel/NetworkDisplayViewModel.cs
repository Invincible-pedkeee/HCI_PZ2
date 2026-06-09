using System.Collections.ObjectModel;
using System.Linq;
using NetworkService.Helpers;
using NetworkService.Model;
using NetworkService.Services;

namespace NetworkService.ViewModel
{
    public class NetworkDisplayViewModel : BindableBase
    {
        private readonly NetworkDataService dataService;
        private ObservableCollection<EntitiesByType> availableEntityGroups;

        public ObservableCollection<EntitiesByType> AvailableEntityGroups
        {
            get
            {
                return availableEntityGroups;
            }
            set
            {
                SetProperty(ref availableEntityGroups, value);
            }
        }

        public ObservableCollection<DisplaySlot> DisplaySlots
        {
            get
            {
                return dataService.DisplaySlots;
            }
        }

        public ObservableCollection<HistoryAction> HistoryActions
        {
            get
            {
                return dataService.HistoryActions;
            }
        }

        public NetworkDisplayViewModel(NetworkDataService dataService)
        {
            this.dataService = dataService;

            RefreshAvailableEntityGroups();
        }

        public void RefreshAvailableEntityGroups()
        {
            ObservableCollection<EntitiesByType> groups = new ObservableCollection<EntitiesByType>();

            foreach (EntityType type in dataService.EntityTypes)
            {
                EntitiesByType group = new EntitiesByType(type.Name);

                foreach (NetworkEntity entity in dataService.Entities
                             .Where(entity => entity.Type != null &&
                                              entity.Type.Name == type.Name &&
                                              !entity.IsPlacedOnDisplay))
                {
                    group.Entities.Add(entity);
                }

                groups.Add(group);
            }

            AvailableEntityGroups = groups;
        }

        public void DropEntityToSlot(NetworkEntity entity, DisplaySlot slot)
        {
            if (entity == null || slot == null || slot.IsOccupied)
            {
                return;
            }

            slot.PlaceEntity(entity);

            RefreshAvailableEntityGroups();

            dataService.AddHistory("Entitet ID: " + entity.Id + " postavljen na mrežu, slot " + slot.SlotNumber + ".");
        }

        public void MoveEntityBetweenSlots(DisplaySlot sourceSlot, DisplaySlot targetSlot)
        {
            if (sourceSlot == null || targetSlot == null)
            {
                return;
            }

            if (!sourceSlot.IsOccupied || targetSlot.IsOccupied)
            {
                return;
            }

            if (sourceSlot == targetSlot)
            {
                return;
            }

            NetworkEntity entity = sourceSlot.RemoveEntity();
            targetSlot.PlaceEntity(entity);

            RefreshAvailableEntityGroups();

            dataService.AddHistory(
                "Entitet ID: " + entity.Id +
                " premješten sa slota " + sourceSlot.SlotNumber +
                " na slot " + targetSlot.SlotNumber + ".");
        }

        public void RemoveEntityFromSlot(DisplaySlot slot)
        {
            if (slot == null || !slot.IsOccupied)
            {
                return;
            }

            NetworkEntity removedEntity = slot.RemoveEntity();

            RefreshAvailableEntityGroups();

            dataService.AddHistory("Entitet ID: " + removedEntity.Id + " uklonjen sa mreže.");
        }
    }
}