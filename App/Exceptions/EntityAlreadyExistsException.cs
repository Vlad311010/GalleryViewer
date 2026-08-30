namespace App.Exceptions
{
    public class EntityAlreadyExistsException : AppException
    {
        public Type EntityType { get; }
        public string EntityIdentifier { get; protected set; }
        protected EntityAlreadyExistsException(Type entityType, string identifier, string message) : base(message)
        {
            EntityType = entityType;
            EntityIdentifier = identifier;
        }
    }

    internal class EntityAlreadyExistsException<TEnity> : EntityAlreadyExistsException
    {
        public EntityAlreadyExistsException(int entityId) :
            base(typeof(TEnity), entityId.ToString(), $"Entity {typeof(TEnity).Name} with id:{entityId} alredy exists.")
        {

        }

        public EntityAlreadyExistsException(string entityIdentifier) :
            base(typeof(TEnity), entityIdentifier, $"Entity {typeof(TEnity).Name} with identifier:{entityIdentifier} alredy exists.")
        {

        }
    }
}
