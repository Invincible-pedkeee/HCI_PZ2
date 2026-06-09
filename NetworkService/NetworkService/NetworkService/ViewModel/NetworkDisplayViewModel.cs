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
        private DisplaySlot selectedConnectionSlot;
        private string connectionInfoText;

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

        public ObservableCollection<ConnectionLine> Connections
        {
            get
            {
                return dataService.Connections;
            }
        }

        public ObservableCollection<HistoryAction> HistoryActions
        {
            get
            {
                return dataService.HistoryActions;
            }
        }

        public string ConnectionInfoText
        {
            get
            {
                return connectionInfoText;
            }
            set
            {
                SetProperty(ref connectionInfoText, value);
            }
        }

        public NetworkDisplayViewModel(NetworkDataService dataService)
        {
            this.dataService = dataService;

            ConnectionInfoText = "Za kreiranje veze kliknite Poveži na prvom, zatim na drugom entitetu.";
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

            dataService.RemoveConnectionsForEntity(removedEntity);

            if (selectedConnectionSlot == slot)
            {
                ClearSelectedConnectionSlot();
            }

            RefreshAvailableEntityGroups();

            dataService.AddHistory("Entitet ID: " + removedEntity.Id + " uklonjen sa mreže.");
        }

        public void StartOrCompleteConnection(DisplaySlot slot)
        {
            if (slot == null || !slot.IsOccupied)
            {
                ConnectionInfoText = "Veza se može kreirati samo između zauzetih slotova.";
                return;
            }

            if (selectedConnectionSlot == null)
            {
                selectedConnectionSlot = slot;
                ConnectionInfoText = "Izabran prvi entitet ID: " + slot.OccupiedEntity.Id + ". Izaberite drugi entitet.";
                return;
            }

            if (selectedConnectionSlot == slot)
            {
                ClearSelectedConnectionSlot();
                ConnectionInfoText = "Kreiranje veze je poništeno.";
                return;
            }

            NetworkEntity firstEntity = selectedConnectionSlot.OccupiedEntity;
            NetworkEntity secondEntity = slot.OccupiedEntity;

            if (ConnectionAlreadyExists(firstEntity, secondEntity))
            {
                ClearSelectedConnectionSlot();
                ConnectionInfoText = "Veza između izabranih entiteta već postoji.";
                dataService.AddHistory("Pokušaj kreiranja duple veze je odbijen.");
                return;
            }

            dataService.Connections.Add(new ConnectionLine(firstEntity, secondEntity));

            dataService.AddHistory(
                "Kreirana veza između entiteta ID: " +
                firstEntity.Id + " i ID: " + secondEntity.Id + ".");

            ClearSelectedConnectionSlot();

            ConnectionInfoText = "Veza je uspješno kreirana.";
        }

        private bool ConnectionAlreadyExists(NetworkEntity firstEntity, NetworkEntity secondEntity)
        {
            return dataService.Connections.Any(connection => connection.Connects(firstEntity, secondEntity));
        }

        private void ClearSelectedConnectionSlot()
        {
            selectedConnectionSlot = null;
        }
    }
}