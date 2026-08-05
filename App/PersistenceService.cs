using Data.Entities;

namespace App
{
    public class PersistenceService
    {
        protected readonly AssetsCatalogContext context;

        public PersistenceService(AssetsCatalogContext context)
        {
            this.context = context;
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
