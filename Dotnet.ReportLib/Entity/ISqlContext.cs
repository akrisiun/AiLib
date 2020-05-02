using System;
using System.Data;
using System.Data.SqlClient;

namespace Dotnet.Entity
{
    public interface IDbContext : ILastError, IDisposable
    {
        IDbConnection Connection { get; }
        string ConnKey { get; set; }
        string DbName { get; }

        string ConnectionString();
        bool AssureOpen(bool withCommand = false);
    }

    public interface ISqlContext : IDbContext, ILastError, IDisposable
    {
        SqlConnection SqlConnection { get; }

        void SetProxy(SqlConnection conn);

        int? SPID { get; }
        void UpdateSpid(int? spid);

        EventHandler<SqlConnEventArgs> OnBeforeOpen { get; set; }
        EventHandler<SqlConnEventArgs> OnAfterOpen { get; set; }
    }

    public interface IDataContext
    {
        int CommandTimeout { get; set; }
        IDbConnection Connection { get; set; }
    }
}