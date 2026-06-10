using System;
using System.Collections.ObjectModel;
using System.Linq;
using NetworkService.Model;

namespace NetworkService.Services
{
    public class NetworkDataService
    {
        private readonly MeasurementLogService measurementLogService;
        public ObservableCollection<EntityType> EntityTypes { get; private set; }
        public ObservableCollection<ConnectionLine> Connections { get; private set; }
        public ObservableCollection<DisplaySlot> DisplaySlots { get; private set; }
        public ObservableCollection<NetworkEntity> Entities { get; private set; }

        public ObservableCollection<Measurement> Measurements { get; private set; }

        public ObservableCollection<HistoryAction> HistoryActions { get; private set; }

        public NetworkDataService()
        {
            measurementLogService = new MeasurementLogService();

            EntityTypes = new ObservableCollection<EntityType>();
            Entities = new ObservableCollection<NetworkEntity>();
            Measurements = new ObservableCollection<Measurement>();
            HistoryActions = new ObservableCollection<HistoryAction>();
            DisplaySlots = new ObservableCollection<DisplaySlot>();
            Connections = new ObservableCollection<ConnectionLine>();
            LoadEntityTypes();
            LoadInitialEntities();
            LoadDisplaySlots();

            AddHistory("Sistem pokrenut.");
        }
        private void LoadDisplaySlots()
        {
            for (int i = 1; i <= 12; i++)
            {
                DisplaySlots.Add(new DisplaySlot(i));
            }
        }
        public void RemoveConnectionsForEntity(NetworkEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            for (int i = Connections.Count - 1; i >= 0; i--)
            {
                if (Connections[i].ContainsEntity(entity))
                {
                    Connections.RemoveAt(i);
                }
            }
        }
        private void LoadEntityTypes()
        {
            EntityTypes.Add(new EntityType("IA", "/Resources/Images/road_ia.png", 15000));
            EntityTypes.Add(new EntityType("IB", "/Resources/Images/road_ib.png", 7000));
        }

        private void LoadInitialEntities()
        {
            Entities.Add(new NetworkEntity(1, "Autoput E75", EntityTypes[0], 0));
            Entities.Add(new NetworkEntity(2, "Magistralni put M17", EntityTypes[1], 0));
            Entities.Add(new NetworkEntity(3, "Put Novi Sad - Beograd", EntityTypes[0], 0));
        }

        public bool IsIdUnique(int id)
        {
            return !Entities.Any(entity => entity.Id == id);
        }

        public NetworkEntity GetEntityById(int id)
        {
            return Entities.FirstOrDefault(entity => entity.Id == id);
        }

        public NetworkEntity GetEntityBySimulatorIndex(int simulatorIndex)
        {
            if (simulatorIndex < 0 || simulatorIndex >= Entities.Count)
            {
                return null;
            }

            return Entities[simulatorIndex];
        }

        public void AddEntity(NetworkEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            Entities.Add(entity);
            AddHistory("Dodat novi entitet ID: " + entity.Id);
        }

        public void DeleteEntity(NetworkEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            foreach (DisplaySlot slot in DisplaySlots)
            {
                if (slot.OccupiedEntity == entity)
                {
                    slot.RemoveEntity();
                }
            }

            RemoveConnectionsForEntity(entity);

            Entities.Remove(entity);
            AddHistory("Obrisan entitet ID: " + entity.Id);
        }

        public Measurement AddMeasurement(NetworkEntity entity, double value)
        {
            if (entity == null)
            {
                return null;
            }

            entity.LastValue = value;

            Measurement measurement = new Measurement(
                entity.Id,
                value,
                DateTime.Now,
                entity.IsValueValid);

            Measurements.Add(measurement);
            measurementLogService.WriteMeasurement(measurement, entity);

            AddHistory("Primljeno mjerenje za ID: " + entity.Id + ", vrijednost: " + value);

            return measurement;
        }

        public ObservableCollection<Measurement> GetLastMeasurementsForEntity(int entityId, int count)
        {
            var lastMeasurements = Measurements
                .Where(measurement => measurement.EntityId == entityId)
                .OrderByDescending(measurement => measurement.Timestamp)
                .Take(count)
                .OrderBy(measurement => measurement.Timestamp)
                .ToList();

            return new ObservableCollection<Measurement>(lastMeasurements);
        }

        public void AddHistory(string description)
        {
            HistoryActions.Insert(0, new HistoryAction(description));

            if (HistoryActions.Count > 100)
            {
                HistoryActions.RemoveAt(HistoryActions.Count - 1);
            }
        }


    }
}