namespace App.Interfaces
{
    internal interface IDbContextWrapper
    {
        public Task SaveChangesAsync();
    }
}
