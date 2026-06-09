namespace NetworkService.Model
{
    public class EntityFilterState
    {
        public EntityType SelectedFilterType { get; set; }

        public string SelectedIdComparison { get; set; }

        public string FilterIdText { get; set; }

        public string SelectedStatusFilter { get; set; }

        public EntityFilterState()
        {
        }

        public EntityFilterState(
            EntityType selectedFilterType,
            string selectedIdComparison,
            string filterIdText,
            string selectedStatusFilter)
        {
            SelectedFilterType = selectedFilterType;
            SelectedIdComparison = selectedIdComparison;
            FilterIdText = filterIdText;
            SelectedStatusFilter = selectedStatusFilter;
        }
    }
}