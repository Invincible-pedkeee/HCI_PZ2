using System.Collections.ObjectModel;

namespace NetworkService.Model
{
    public class EntitiesByType
    {
        public string TypeName { get; set; }

        public ObservableCollection<NetworkEntity> Entities { get; set; }

        public EntitiesByType()
        {
            Entities = new ObservableCollection<NetworkEntity>();
        }

        public EntitiesByType(string typeName)
        {
            TypeName = typeName;
            Entities = new ObservableCollection<NetworkEntity>();
        }
    }
}