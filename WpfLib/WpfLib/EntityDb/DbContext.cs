using Ai.Entity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Ai.Entity
{
    // Simple database context
    public class DbContext : Context
    {
        public DbContext(string connKey, string database)
        {
            if (connKey.StartsWith("name="))
            {
                string name = connKey.Replace("name=", "");
                this.connectionString =
                     ConfigurationManager.ConnectionStrings[name].ConnectionString;
            }
            db = database;
        }

        #region Connection
        private string connectionString;
        private string db;
        public string ServerName { get { return this.SqlConnection.DataSource; } }
        public override string DbName { get { return db; } }

        public override string ConnectionString()
        {
            return connectionString;
        }

        public override SqlConnection SqlConnection
        {
            get
            {
                var conn = base.Connection as SqlConnection;
                if (string.IsNullOrEmpty(conn.ConnectionString))
                    conn.ConnectionString = this.ConnectionString();
                return conn;
            }

        }

        public bool AssureConnection()
        {
            SqlConnection conn = SqlConnection;
            try
            {
                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();

                // DbContext.cs:line 53 Error There is already an open DataReader associated with this Command 
                // add "MultipleActiveResultSets=True;"
                conn.ChangeDatabase(DbName);
            }
            catch (Exception ex)
            {
                Ai.Log.WriteException("DbContext.AssureConnection error", ex);
                conn.Close();
                conn = null;
            }

            if (conn == null)
            {
                // after Connection .Close()
                conn = SqlConnection;
                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();
                conn.ChangeDatabase(DbName);
            }

            return (conn.State == System.Data.ConnectionState.Open);
        }

        public override bool AssureOpen()
        {
            return base.AssureOpen();
        }

        #endregion
    }
}
