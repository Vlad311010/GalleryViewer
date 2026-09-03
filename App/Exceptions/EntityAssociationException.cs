namespace App.Exceptions
{
    public class EntityAssociationException : AppException
    {
        public Type EntityType { get; }
        public string EntityIdentifier { get; }
        public Type AssociateEntityType { get; }
        public string AssociateEntityIdentifier { get; }

        protected EntityAssociationException(Type entityType, string entityIdentifier, Type associateEntityType, string associateEntityIdentifier, string message) : base(message)
        {
            EntityType = entityType;
            EntityIdentifier = entityIdentifier;
            AssociateEntityType = associateEntityType;
            AssociateEntityIdentifier = associateEntityIdentifier;
        }
    }

    internal class EntityAssociationException<TEnity, TAssciateEntity> : EntityAssociationException
    {
        public EntityAssociationException(int entityId, int associatedEntityId, string message)
            : base(typeof(TEnity), entityId.ToString(), typeof(TAssciateEntity), associatedEntityId.ToString(), message)
        {

        }
    }
}
