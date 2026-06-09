using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NetworkService.Helpers;
 
using NetworkService.Helpers.Undo;
using NetworkService.Model;
using NetworkService.Services;

namespace NetworkService.ViewModel
{
    public class NetworkDisplayViewModel : BindableBase
    {
        private readonly NetworkDataService dataService;
        private readonly Stack<UndoAction> undoActions;

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

        public MyICommand UndoCommand { get; private set; }

        public MyICommand UndoAllCommand { get; private set; }

        public NetworkDisplayViewModel(NetworkDataService dataService)
        {
            this.dataService = dataService;
            undoActions = new Stack<UndoAction>();

            UndoCommand = new MyICommand(OnUndo, CanUndo);
            UndoAllCommand = new MyICommand(OnUndoAll, CanUndo);

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

            RegisterUndoAction(new UndoAction(
                "postavljanje entiteta ID: " + entity.Id + " na slot " + slot.SlotNumber,
                () =>
                {
                    DisplaySlot currentSlot = FindSlotForEntity(entity);

                    if (currentSlot != null)
                    {
                        currentSlot.RemoveEntity();
                        dataService.RemoveConnectionsForEntity(entity);
                    }

                    RefreshAvailableEntityGroups();
                }));

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

            int sourceSlotNumber = sourceSlot.SlotNumber;
            int targetSlotNumber = targetSlot.SlotNumber;

            targetSlot.PlaceEntity(entity);

            RegisterUndoAction(new UndoAction(
                "premještanje entiteta ID: " + entity.Id,
                () =>
                {
                    DisplaySlot currentSlot = FindSlotForEntity(entity);
                    DisplaySlot originalSlot = FindSlotByNumber(sourceSlotNumber);

                    if (currentSlot != null && originalSlot != null && !originalSlot.IsOccupied)
                    {
                        currentSlot.RemoveEntity();
                        originalSlot.PlaceEntity(entity);
                    }

                    RefreshAvailableEntityGroups();
                }));

            RefreshAvailableEntityGroups();

            dataService.AddHistory(
                "Entitet ID: " + entity.Id +
                " premješten sa slota " + sourceSlotNumber +
                " na slot " + targetSlotNumber + ".");
        }

        public void RemoveEntityFromSlot(DisplaySlot slot)
        {
            if (slot == null || !slot.IsOccupied)
            {
                return;
            }

            NetworkEntity removedEntity = slot.OccupiedEntity;
            int slotNumber = slot.SlotNumber;

            List<ConnectionLine> removedConnections = dataService.Connections
                .Where(connection => connection.ContainsEntity(removedEntity))
                .ToList();

            slot.RemoveEntity();
            dataService.RemoveConnectionsForEntity(removedEntity);

            if (selectedConnectionSlot == slot)
            {
                ClearSelectedConnectionSlot();
            }

            RegisterUndoAction(new UndoAction(
                "uklanjanje entiteta ID: " + removedEntity.Id + " sa mreže",
                () =>
                {
                    DisplaySlot originalSlot = FindSlotByNumber(slotNumber);

                    if (originalSlot != null && !originalSlot.IsOccupied)
                    {
                        originalSlot.PlaceEntity(removedEntity);
                        RestoreConnections(removedConnections);
                    }

                    RefreshAvailableEntityGroups();
                }));

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

            ConnectionLine connection = new ConnectionLine(firstEntity, secondEntity);
            dataService.Connections.Add(connection);

            RegisterUndoAction(new UndoAction(
                "kreiranje veze ID: " + firstEntity.Id + " - ID: " + secondEntity.Id,
                () =>
                {
                    dataService.Connections.Remove(connection);
                }));

            dataService.AddHistory(
                "Kreirana veza između entiteta ID: " +
                firstEntity.Id + " i ID: " + secondEntity.Id + ".");

            ClearSelectedConnectionSlot();

            ConnectionInfoText = "Veza je uspješno kreirana.";
        }

        private bool CanUndo()
        {
            return undoActions.Count > 0;
        }

        private void OnUndo()
        {
            if (!CanUndo())
            {
                return;
            }

            UndoAction undoAction = undoActions.Pop();
            undoAction.Execute();

            dataService.AddHistory("Undo izvršen na prikazu mreže: " + undoAction.Description);

            RefreshUndoCommands();
        }

        private void OnUndoAll()
        {
            if (!CanUndo())
            {
                return;
            }

            int actionCount = undoActions.Count;

            while (undoActions.Count > 0)
            {
                UndoAction undoAction = undoActions.Pop();
                undoAction.Execute();
            }

            dataService.AddHistory("Undo All izvršen na prikazu mreže. Broj poništenih akcija: " + actionCount);

            RefreshUndoCommands();
        }

        private void RegisterUndoAction(UndoAction undoAction)
        {
            undoActions.Push(undoAction);
            RefreshUndoCommands();
        }

        private void RefreshUndoCommands()
        {
            UndoCommand.RaiseCanExecuteChanged();
            UndoAllCommand.RaiseCanExecuteChanged();
        }

        private DisplaySlot FindSlotForEntity(NetworkEntity entity)
        {
            return dataService.DisplaySlots.FirstOrDefault(slot => slot.OccupiedEntity == entity);
        }

        private DisplaySlot FindSlotByNumber(int slotNumber)
        {
            return dataService.DisplaySlots.FirstOrDefault(slot => slot.SlotNumber == slotNumber);
        }

        private bool ConnectionAlreadyExists(NetworkEntity firstEntity, NetworkEntity secondEntity)
        {
            return dataService.Connections.Any(connection => connection.Connects(firstEntity, secondEntity));
        }

        private void RestoreConnections(List<ConnectionLine> connections)
        {
            foreach (ConnectionLine connection in connections)
            {
                if (!dataService.Entities.Contains(connection.FirstEntity) ||
                    !dataService.Entities.Contains(connection.SecondEntity))
                {
                    continue;
                }

                if (!ConnectionAlreadyExists(connection.FirstEntity, connection.SecondEntity))
                {
                    dataService.Connections.Add(connection);
                }
            }
        }

        private void ClearSelectedConnectionSlot()
        {
            selectedConnectionSlot = null;
        }
    }
}