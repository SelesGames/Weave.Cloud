using System.Data.Linq;

namespace Common.Data.Linq
{
    public abstract class LinqDbClient
    {
        readonly string connectionString;

        public LinqDbClient(SqlServerCredentials credentials)
        {
            this.connectionString = credentials.GetConnectionString();
        }

        protected DataContext CreateContext()
        {
#if DEBUG
            return new DataContext(connectionString) { ObjectTrackingEnabled = true, Log = new DebugTextWriter() };
#else
            return new DataContext(connectionString) { ObjectTrackingEnabled = true };
#endif
        }

        protected DataContext CreateReadOnlyContext()
        {
#if DEBUG
            return new DataContext(connectionString) { ObjectTrackingEnabled = false, Log = new DebugTextWriter() };
#else
            return new DataContext(connectionString) { ObjectTrackingEnabled = true };
#endif
        }
    }
}
