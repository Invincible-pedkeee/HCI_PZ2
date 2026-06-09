using System.Collections.ObjectModel;
using System.Linq;
using NetworkService.Helpers;

using NetworkService.Model;
using NetworkService.Services;

namespace NetworkService.ViewModel
{
    public class NetworkEntitiesViewModel : BindableBase
    {
        private readonly NetworkDataService dataService;

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

        public ObservableCollection<NetworkEntity> Entities
        {
            get
            {
                return dataService.Entities;
            }
        }

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

        public NetworkEntitiesViewModel(NetworkDataService dataService)
        {
            this.dataService = dataService;

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

            FilteredEntities = new ObservableCollection<NetworkEntity>(dataService.Entities);

            AddEntityCommand = new MyICommand(OnAddEntity);
            RequestDeleteEntityCommand = new MyICommand(OnRequestDeleteEntity, CanDeleteEntity);
            ConfirmDeleteEntityCommand = new MyICommand(OnConfirmDeleteEntity);
            CancelDeleteEntityCommand = new MyICommand(OnCancelDeleteEntity);

            ApplyFilterCommand = new MyICommand(OnApplyFilter);
            ClearFilterCommand = new MyICommand(OnClearFilter);
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

            CurrentEntityInput = new NetworkEntityInput(dataService.Entities);
            RefreshFilteredEntities(dataService.Entities);
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

            dataService.DeleteEntity(SelectedEntity);
            SelectedEntity = null;
            IsDeleteDialogVisible = false;

            OnApplyFilter();
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

            var query = dataService.Entities.AsEnumerable();

            if (SelectedFilterType != null)
            {
                query = query.Where(entity => entity.Type != null &&
                                              entity.Type.Name == SelectedFilterType.Name);
            }

            if (!string.IsNullOrWhiteSpace(FilterIdText))
            {
                int idValue = int.Parse(FilterIdText);

                if (SelectedIdComparison == "<")
                {
                    query = query.Where(entity => entity.Id < idValue);
                }
                else if (SelectedIdComparison == ">")
                {
                    query = query.Where(entity => entity.Id > idValue);
                }
                else if (SelectedIdComparison == "=")
                {
                    query = query.Where(entity => entity.Id == idValue);
                }
            }

            if (SelectedStatusFilter == "Unutar opsega")
            {
                query = query.Where(entity => entity.IsValueValid);
            }
            else if (SelectedStatusFilter == "Van opsega")
            {
                query = query.Where(entity => !entity.IsValueValid);
            }

            RefreshFilteredEntities(query);

            dataService.AddHistory("Primijenjen filter nad tabelom entiteta.");
        }

        private void OnClearFilter()
        {
            SelectedFilterType = null;
            SelectedIdComparison = null;
            FilterIdText = string.Empty;
            SelectedStatusFilter = "Sva stanja";

            ClearFilterErrors();
            RefreshFilteredEntities(dataService.Entities);

            dataService.AddHistory("Poništeni svi filteri u tabeli entiteta.");
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

        private void RefreshFilteredEntities(System.Collections.Generic.IEnumerable<NetworkEntity> entities)
        {
            FilteredEntities = new ObservableCollection<NetworkEntity>(entities);
        }
    }
}