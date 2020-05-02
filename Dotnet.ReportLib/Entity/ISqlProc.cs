using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Dotnet.Entity
{
    public interface IConn
    {
        bool PrepareConn();
        void DisposeConn();
    }

    public interface ISqlProc : IDisposable, ILastError
    {
        IDbConnection Connection { get; set; }
        bool CloseOnDispose { get; set; }

        string CmdText { get; set; }
        IDbCommand LastCommand { get; }
        IDbCommand CreateCommand();
        IDbConnection OpenConnection();
        string ConnectionString();
        string DbName { get; }

        IDataReader ExecuteReader(IDbCommand cmd);
        Task<SqlDataReader> ExecuteReaderAsync(IDbCommand cmd);
    }

    public interface ISqlProcAsync : ISqlProc
    {
        Task OpenAsync();
        Task<bool> PrepareAsync();
    }

    public interface ISqlProcReader : ISqlProc
    {
        IDataReader Reader { get; }
        IList<SqlParameter> Param { get; set; }
    }

    public interface ISqlProcContext : ISqlProc
    {
        ISqlContext Context { get; set; }
    }

    public struct SqlProcData
    {
        public SqlConnection Connection { get; set; }
        public ISqlContext Context { get; set; }

        public string CmdText { get; set; }
        public IList<SqlParameter> Param { get; set; }
    }

}
