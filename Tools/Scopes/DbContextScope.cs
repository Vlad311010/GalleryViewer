using Microsoft.EntityFrameworkCore;

namespace Tools.Scopes
{
    internal class DbContextScope<T> : IDisposable where T : DbContext
    {
        private bool isDisposed;

        protected T Context { get; }

        public DbContextScope(T context)
        {
            this.Context = context;
        }

        ~DbContextScope() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;

                if (disposing)
                {
                    Context?.Dispose();
                }
            }

            isDisposed = true;
        }
    }
}
