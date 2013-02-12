using System;

namespace Common.Data
{
    public class SqlServerCredentials
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string ConnectionString { get; set; }

        public string GetConnectionString()
        {
            if (!string.IsNullOrEmpty(ConnectionString))
                return ConnectionString;

            else
                throw new Exception("Building a connection string from parameters is not currently supported");
        }
    }
}
