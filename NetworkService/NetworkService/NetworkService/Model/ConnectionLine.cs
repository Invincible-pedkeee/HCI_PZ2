namespace NetworkService.Model
{
    public class ConnectionLine
    {
        public NetworkEntity FirstEntity { get; set; }

        public NetworkEntity SecondEntity { get; set; }

        public ConnectionLine()
        {
        }

        public ConnectionLine(NetworkEntity firstEntity, NetworkEntity secondEntity)
        {
            FirstEntity = firstEntity;
            SecondEntity = secondEntity;
        }

        public bool Connects(NetworkEntity firstEntity, NetworkEntity secondEntity)
        {
            return (FirstEntity == firstEntity && SecondEntity == secondEntity) ||
                   (FirstEntity == secondEntity && SecondEntity == firstEntity);
        }

        public bool ContainsEntity(NetworkEntity entity)
        {
            return FirstEntity == entity || SecondEntity == entity;
        }
    }
}