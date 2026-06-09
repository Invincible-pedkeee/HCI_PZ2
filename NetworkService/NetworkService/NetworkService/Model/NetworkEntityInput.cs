using NetworkService.Helpers;

using System.Collections.ObjectModel;
using System.Linq;

namespace NetworkService.Model
{
    public class NetworkEntityInput : ValidationBase
    {
        private readonly ObservableCollection<NetworkEntity> existingEntities;

        private string idText;
        private string name;
        private EntityType selectedType;

        public string IdText
        {
            get
            {
                return idText;
            }
            set
            {
                SetProperty(ref idText, value);
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

        public EntityType SelectedType
        {
            get
            {
                return selectedType;
            }
            set
            {
                SetProperty(ref selectedType, value);
            }
        }

        public NetworkEntityInput(ObservableCollection<NetworkEntity> existingEntities)
        {
            this.existingEntities = existingEntities;
        }

        public bool TryGetId(out int id)
        {
            return int.TryParse(IdText, out id);
        }

        protected override void ValidateSelf()
        {
            int parsedId;

            if (string.IsNullOrWhiteSpace(IdText))
            {
                ValidationErrors["IdText"] = "ID is required.";
            }
            else if (!int.TryParse(IdText, out parsedId))
            {
                ValidationErrors["IdText"] = "ID must be a whole number.";
            }
            else if (parsedId <= 0)
            {
                ValidationErrors["IdText"] = "ID must be greater than zero.";
            }
            else if (existingEntities.Any(entity => entity.Id == parsedId))
            {
                ValidationErrors["IdText"] = "ID must be unique.";
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                ValidationErrors["Name"] = "Road name is required.";
            }

            if (SelectedType == null)
            {
                ValidationErrors["SelectedType"] = "Road type must be selected.";
            }
        }
    }
}