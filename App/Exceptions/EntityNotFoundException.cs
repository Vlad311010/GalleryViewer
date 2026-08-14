using System.Diagnostics.CodeAnalysis;

namespace App.Exceptions
{
    internal class EntityNotFoundException<TEnity> : AppException
    {
        public int? EntityId { get; }

        public EntityNotFoundException(string message, int entityId) : base(message)
        {
            EntityId = entityId;
        }

        public EntityNotFoundException(int entityId) : base($"Entity {typeof(TEnity).Name} with id:{entityId} not found.")
        {
            EntityId = entityId;
        }

        public EntityNotFoundException(string entityIdentifier) : base($"Entity {typeof(TEnity).Name} with identifier:{entityIdentifier} not found.")
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
