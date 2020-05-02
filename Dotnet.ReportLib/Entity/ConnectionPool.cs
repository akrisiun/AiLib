using System;
using System.Data;
using System.Data.SqlClient;

namespace Dotnet.Entity
{
    // Connection create and dispose

    public static class ConnectionPool
    {
        public static SqlConnection NewConn(string connectionString)
        {
            SqlConnection conn = null;
           
            conn = new SqlConnection(connectionString);
            if (!string.IsNullOrWhiteSpace(conn.DataSource))
                Context.SetLastConnString(connectionString);

            return conn;
        }

        public static void CloseConn(this IDbConnection connection, bool withPool = true, ILastError onError = null)
        {
            if (connection == null)
                return;
            try
            {
                connection.Dispose();
                if (withPool && connection is SqlConnection)
                    SqlConnection.ClearPool(connection as SqlConnection);
            }
            catch (Exception ex)
            {
                // DbObject PrePush internal exception 
                if (onError != null) onError.LastError = ex;
            }
        }
    }

    public static class SqlPoolInfo
    {
        // http://blah.winsmarts.com/2007-3-Determining_number_of_open_connections_in_your_connection_pools.aspx
    }
}
