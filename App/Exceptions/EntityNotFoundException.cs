using System.Diagnostics.CodeAnalysis;

namespace App.Exceptions
{
    public class EntityNotFoundException : AppException
    {
        public Type EntityType { get; }
        public string EntityIdentifier { get; protected set; }
        protected EntityNotFoundException(Type entityType, string identifier, string message) : base(message)
        {
            EntityType = entityType;
            EntityIdentifier = identifier;
        }
    }

    internal class EntityNotFoundException<TEnity> : EntityNotFoundException
    {
        public EntityNotFoundException(int entityId) : base(typeof(TEnity), entityId.ToString(), $"Entity {typeof(TEnity).Name} with id:{entityId} not found.")
        {

        }

        public EntityNotFoundException(string entityIdentifier) : base(typeof(TEnity), entityIdentifier, $"Entity {typeof(TEnity).Name} with identifier:{entityIdentifier} not found.")
        {

        }

        public static void ThrowIfNull<T>([NotNull] T? entity, int id)
        {
            if (entity == null)
                throw new EntityNotFoundException<T>(id);
        }

        public static void ThrowIfNull<T>([NotNull] T? entity, string identifier)
        {
            if (entity == null)
                throw new EntityNotFoundException<T>(identifier);
        }
    }
}
