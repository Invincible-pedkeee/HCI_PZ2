using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NetworkService.Helpers;
 
using NetworkService.Helpers.Undo;
using NetworkService.Model;
using NetworkService.Services;

namespace NetworkService.ViewModel
{
    public class NetworkEntitiesViewModel : BindableBase
    {
        private readonly NetworkDataService dataService;
        private readonly Stack<UndoAction> undoActions;

        private NetworkEntityInput currentEntityInput;
        private NetworkEntity selectedEntity;
        private bool isDeleteDialogVisible;

        private ObservableCollection<NetworkEntity> filteredEntities;

        private EntityType selectedFilterType;
        private string selectedIdComparison;
        private string filterIdText;
        private string selectedStatusFilter;

        private string filterIdError;
        private string filterComparisonError;

        private EntityFilterState appliedFilterState;

        public ObservableCollection<NetworkEntity> FilteredEntities
        {
            get
            {
                return filteredEntities;
            }
            set
            {
                SetProperty(ref filteredEntities, value);
            }
        }

        public ObservableCollection<EntityType> EntityTypes
        {
            get
            {
                return dataService.EntityTypes;
            }
        }

        public ObservableCollection<HistoryAction> HistoryActions
        {
            get
            {
                return dataService.HistoryActions;
            }
        }

        public ObservableCollection<string> IdComparisonOptions { get; private set; }

        public ObservableCollection<string> StatusFilterOptions { get; private set; }

        public NetworkEntityInput CurrentEntityInput
        {
            get
            {
                return currentEntityInput;
            }
            set
            {
                SetProperty(ref currentEntityInput, value);
            }
        }

        public NetworkEntity SelectedEntity
        {
            get
            {
                return selectedEntity;
            }
            set
            {
                SetProperty(ref selectedEntity, value);
                RequestDeleteEntityCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsDeleteDialogVisible
        {
            get
            {
                return isDeleteDialogVisible;
            }
            set
            {
                SetProperty(ref isDeleteDialogVisible, value);
            }
        }

        public EntityType SelectedFilterType
        {
            get
            {
                return selectedFilterType;
            }
            set
            {
                SetProperty(ref selectedFilterType, value);
            }
        }

        public string SelectedIdComparison
        {
            get
            {
                return selectedIdComparison;
            }
            set
            {
                SetProperty(ref selectedIdComparison, value);
            }
        }

        public string FilterIdText
        {
            get
            {
                return filterIdText;
            }
            set
            {
                SetProperty(ref filterIdText, value);
            }
        }

        public string SelectedStatusFilter
        {
            get
            {
                return selectedStatusFilter;
            }
            set
            {
                SetProperty(ref selectedStatusFilter, value);
            }
        }

        public string FilterIdError
        {
            get
            {
                return filterIdError;
            }
            set
            {
                SetProperty(ref filterIdError, value);
            }
        }

        public string FilterComparisonError
        {
            get
            {
                return filterComparisonError;
            }
            set
            {
                SetProperty(ref filterComparisonError, value);
            }
        }

        public MyICommand AddEntityCommand { get; private set; }

        public MyICommand RequestDeleteEntityCommand { get; private set; }

        public MyICommand ConfirmDeleteEntityCommand { get; private set; }

        public MyICommand CancelDeleteEntityCommand { get; private set; }

        public MyICommand ApplyFilterCommand { get; private set; }

        public MyICommand ClearFilterCommand { get; private set; }

        public MyICommand UndoCommand { get; private set; }

        public MyICommand UndoAllCommand { get; private set; }

        public NetworkEntitiesViewModel(NetworkDataService dataService)
        {
            this.dataService = dataService;
            undoActions = new Stack<UndoAction>();

            CurrentEntityInput = new NetworkEntityInput(dataService.Entities);

            IdComparisonOptions = new ObservableCollection<string>
            {
                "<",
                ">",
                "="
            };

            StatusFilterOptions = new ObservableCollection<string>
            {
                "Sva stanja",
                "Unutar opsega",
                "Van opsega"
            };

            SelectedStatusFilter = "Sva stanja";
            appliedFilterState = CaptureCurrentFilterState();

            FilteredEntities = new ObservableCollection<NetworkEntity>(dataService.Entities);

            AddEntityCommand = new MyICommand(OnAddEntity);
            RequestDeleteEntityCommand = new MyICommand(OnRequestDeleteEntity, CanDeleteEntity);
            ConfirmDeleteEntityCommand = new MyICommand(OnConfirmDeleteEntity);
            CancelDeleteEntityCommand = new MyICommand(OnCancelDeleteEntity);

            ApplyFilterCommand = new MyICommand(OnApplyFilter);
            ClearFilterCommand = new MyICommand(OnClearFilter);

            UndoCommand = new MyICommand(OnUndo, CanUndo);
            UndoAllCommand = new MyICommand(OnUndoAll, CanUndo);
        }

        private void OnAddEntity()
        {
            CurrentEntityInput.Validate();

            if (!CurrentEntityInput.IsValid)
            {
                dataService.AddHistory("Dodavanje entiteta nije uspjelo zbog neispravnog unosa.");
                return;
            }

            int id;
            CurrentEntityInput.TryGetId(out id);

            NetworkEntity entity = new NetworkEntity(
                id,
                CurrentEntityInput.Name,
                CurrentEntityInput.SelectedType,
                0);

            dataService.AddEntity(entity);

            RegisterUndoAction(new UndoAction(
                "dodavanje entiteta ID: " + entity.Id,
                () =>
                {
                    dataService.Entities.Remove(entity);
                    SelectedEntity = null;
                    RefreshTableUsingAppliedFilter();
                }));

            CurrentEntityInput = new NetworkEntityInput(dataService.Entities);
            RefreshTableUsingAppliedFilter();
        }

        private bool CanDeleteEntity()
        {
            return SelectedEntity != null;
        }

        private void OnRequestDeleteEntity()
        {
            IsDeleteDialogVisible = true;
        }

        private void OnConfirmDeleteEntity()
        {
            if (SelectedEntity == null)
            {
                IsDeleteDialogVisible = false;
                return;
            }

            NetworkEntity entityToDelete = SelectedEntity;
            int originalIndex = dataService.Entities.IndexOf(entityToDelete);

            dataService.DeleteEntity(entityToDelete);

            RegisterUndoAction(new UndoAction(
                "brisanje entiteta ID: " + entityToDelete.Id,
                () =>
                {
                    RestoreEntityAtIndex(entityToDelete, originalIndex);
                    RefreshTableUsingAppliedFilter();
                }));

            SelectedEntity = null;
            IsDeleteDialogVisible = false;
            RefreshTableUsingAppliedFilter();
        }

        private void OnCancelDeleteEntity()
        {
            IsDeleteDialogVisible = false;
        }

        private void OnApplyFilter()
        {
            ClearFilterErrors();

            if (!IsFilterInputValid())
            {
                dataService.AddHistory("Filter nije primijenjen zbog neispravnog unosa.");
                return;
            }

            EntityFilterState previousFilterState = appliedFilterState;
            EntityFilterState newFilterState = CaptureCurrentFilterState();

            appliedFilterState = newFilterState;
            RefreshTableUsingAppliedFilter();

            RegisterUndoAction(new UndoAction(
                "primjena filtera nad tabelom entiteta",
                () =>
                {
                    RestoreFilterState(previousFilterState);
                }));

            dataService.AddHistory("Primijenjen filter nad tabelom entiteta.");
        }

        private void OnClearFilter()
        {
            EntityFilterState previousFilterState = appliedFilterState;

            SelectedFilterType = null;
            SelectedIdComparison = null;
            FilterIdText = string.Empty;
            SelectedStatusFilter = "Sva stanja";

            ClearFilterErrors();

            appliedFilterState = CaptureCurrentFilterState();
            RefreshTableUsingAppliedFilter();

            RegisterUndoAction(new UndoAction(
                "poništavanje filtera nad tabelom entiteta",
                () =>
                {
                    RestoreFilterState(previousFilterState);
                }));

            dataService.AddHistory("Poništeni svi filteri u tabeli entiteta.");
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

            dataService.AddHistory("Undo izvršen: " + undoAction.Description);

            RefreshUndoCommands();
        }

        private void OnUndoAll()
        {
            if (!CanUndo())
            {
                return;
            }

            int numberOfActions = undoActions.Count;

            while (undoActions.Count > 0)
            {
                UndoAction undoAction = undoActions.Pop();
                undoAction.Execute();
            }

            dataService.AddHistory("Undo All izvršen. Broj poništenih akcija: " + numberOfActions);

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

        private void RestoreEntityAtIndex(NetworkEntity entity, int index)
        {
            if (dataService.Entities.Contains(entity))
            {
                return;
            }

            if (index < 0 || index > dataService.Entities.Count)
            {
                dataService.Entities.Add(entity);
            }
            else
            {
                dataService.Entities.Insert(index, entity);
            }
        }

        private EntityFilterState CaptureCurrentFilterState()
        {
            return new EntityFilterState(
                SelectedFilterType,
                SelectedIdComparison,
                FilterIdText,
                SelectedStatusFilter);
        }

        private void RestoreFilterState(EntityFilterState filterState)
        {
            SelectedFilterType = filterState.SelectedFilterType;
            SelectedIdComparison = filterState.SelectedIdComparison;
            FilterIdText = filterState.FilterIdText;
            SelectedStatusFilter = filterState.SelectedStatusFilter;

            appliedFilterState = filterState;

            ClearFilterErrors();
            RefreshTableUsingAppliedFilter();
        }

        private void RefreshTableUsingAppliedFilter()
        {
            var query = CreateFilteredQuery(appliedFilterState);
            RefreshFilteredEntities(query);
        }

        private IEnumerable<NetworkEntity> CreateFilteredQuery(EntityFilterState filterState)
        {
            var query = dataService.Entities.AsEnumerable();

            if (filterState.SelectedFilterType != null)
            {
                query = query.Where(entity => entity.Type != null &&
                                              entity.Type.Name == filterState.SelectedFilterType.Name);
            }

            if (!string.IsNullOrWhiteSpace(filterState.FilterIdText))
            {
                int idValue = int.Parse(filterState.FilterIdText);

                if (filterState.SelectedIdComparison == "<")
                {
                    query = query.Where(entity => entity.Id < idValue);
                }
                else if (filterState.SelectedIdComparison == ">")
                {
                    query = query.Where(entity => entity.Id > idValue);
                }
                else if (filterState.SelectedIdComparison == "=")
                {
                    query = query.Where(entity => entity.Id == idValue);
                }
            }

            if (filterState.SelectedStatusFilter == "Unutar opsega")
            {
                query = query.Where(entity => entity.IsValueValid);
            }
            else if (filterState.SelectedStatusFilter == "Van opsega")
            {
                query = query.Where(entity => !entity.IsValueValid);
            }

            return query;
        }

        private bool IsFilterInputValid()
        {
            bool isValid = true;
            int parsedId;

            if (!string.IsNullOrWhiteSpace(FilterIdText) &&
                string.IsNullOrWhiteSpace(SelectedIdComparison))
            {
                FilterComparisonError = "Select ID comparison.";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(FilterIdText) &&
                !string.IsNullOrWhiteSpace(SelectedIdComparison))
            {
                FilterIdError = "Enter ID value.";
                isValid = false;
            }

            if (!string.IsNullOrWhiteSpace(FilterIdText) &&
                !int.TryParse(FilterIdText, out parsedId))
            {
                FilterIdError = "ID filter must be a whole number.";
                isValid = false;
            }

            return isValid;
        }

        private void ClearFilterErrors()
        {
            FilterIdError = string.Empty;
            FilterComparisonError = string.Empty;
        }

        private void RefreshFilteredEntities(IEnumerable<NetworkEntity> entities)
        {
            FilteredEntities = new ObservableCollection<NetworkEntity>(entities);
        }
    }
}