using App.Interfaces.Services;
using Data.Context;

namespace App.Services
{
    public class PersistenceService : IPersistenceService
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
