using System;
using System.Collections.Generic;
using System.Text;

namespace DbFrame.Core.Achieve
{
    using Dapper;
    using DbFrame.BaseClass;
    using DbFrame.Core.Abstract;
    using System.Data;
    using System.Threading.Tasks;

    public class AdoAchieve : AbstractAdo
    {

        public AdoAchieve(string ConnectionString)
            : base(ConnectionString)
        {

        }

        /// <summary>
        /// 数据库 连接对象
        /// </summary>
        /// <returns></returns>
        public override IDbConnection GetDbConnection()
        {
            throw new DbFrameException(" public override IDbConnection GetDbConnection() 数据库 连接对象 NULL ！");
        }

        #region Ado

        public override bool Commit(Action _Action)
        {
            using (this._DbConnection = this.GetDbConnection())
            {
                if (this._DbConnection.State == ConnectionState.Closed) this._DbConnection.Open();
                using (this._DbTransaction = this._DbConnection.BeginTransaction())
                {
                    try
                    {
                        //事务 状态设置 开
                        this.CommitState = true;
                        _Action?.Invoke();
                        if (this._DbTransaction != null) this._DbTransaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        if (this._DbTransaction != null) this._DbTransaction.Rollback();
                        this._DbConnection.Close();
                        //事务 状态设置 关
                        this.CommitState = false;
                        throw ex;
                    }
                    finally
                    {
                        this._DbConnection.Close();
                        //事务 状态设置 关
                        this.CommitState = false;
                    }
                }
            }
        }

        public override bool Commit(Action<List<SQL>> _Action)
        {
            using (var dbConnection = this.GetDbConnection())
            {
                if (dbConnection.State == ConnectionState.Closed) dbConnection.Open();
                using (var _dbTransaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        var _SqlList = new List<SQL>();
                        _Action?.Invoke(_SqlList);

                        foreach (var item in _SqlList) dbConnection.Execute(item.Code.ToString(), item.GetDynamicParameters(), _dbTransaction);

                        _dbTransaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _dbTransaction.Rollback();
                        dbConnection.Close();
                        throw ex;
                    }
                    finally
                    {
                        dbConnection.Close();
                    }
                }
            }
        }

        public override int Execute(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return this.GetDbConnection().Execute(sql, param, transaction, commandTimeout, commandType);
        }

        public override object ExecuteScalar(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return this.GetDbConnection().ExecuteScalar(sql, param, transaction, commandTimeout, commandType);
        }

        public override T ExecuteScalar<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return this.GetDbConnection().ExecuteScalar<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public override IEnumerable<T> Query<T>(string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (typeof(T).Name == new Dictionary<string, object>().GetType().Name)
                return (IEnumerable<T>)(this.QueryDataTable(sql, param, transaction, commandTimeout, commandType).ToList());
            return this.GetDbConnection().Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);
        }

        public override IDataReader ExecuteReader(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return this.GetDbConnection().ExecuteReader(sql, param, transaction, commandTimeout, commandType);
        }

        public override DataTable QueryDataTable(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return this.ExecuteReader(sql, param, transaction, commandTimeout, commandType).ToDataTable();
        }

        public override T QuerySingleOrDefault<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return this.GetDbConnection().QuerySingleOrDefault<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public override T QueryFirstOrDefault<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return this.GetDbConnection().QueryFirstOrDefault<T>(sql, param, transaction, commandTimeout, commandType);
        }

        #endregion


        #region Async

        public override Task<bool> CommitAsync(Action _Action)
        {
            return Task.FromResult(this.Commit(_Action));
        }

        public override Task<bool> CommitAsync(Action<List<SQL>> _Action)
        {
            return Task.FromResult(this.Commit(_Action));
        }

        public override async Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await this.GetDbConnection().ExecuteAsync(sql, param, transaction, commandTimeout, commandType);
        }

        public override async Task<object> ExecuteScalarAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await this.GetDbConnection().ExecuteScalarAsync(sql, param, transaction, commandTimeout, commandType);
        }

        public override async Task<T> ExecuteScalarAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await this.GetDbConnection().ExecuteScalarAsync<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public override async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (typeof(T).Name == new Dictionary<string, object>().GetType().Name)
                return (IEnumerable<T>)((await this.QueryDataTableAsync(sql, param, transaction, commandTimeout, commandType)).ToList());
            return await this.GetDbConnection().QueryAsync<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public override async Task<IDataReader> ExecuteReaderAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await this.GetDbConnection().ExecuteReaderAsync(sql, param, transaction, commandTimeout, commandType);
        }

        public override async Task<DataTable> QueryDataTableAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var _IDataReader = await this.ExecuteReaderAsync(sql, param, transaction, commandTimeout, commandType);
            return _IDataReader.ToDataTable();
        }

        public override async Task<T> QuerySingleOrDefaultAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await this.GetDbConnection().QuerySingleOrDefaultAsync<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public override async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await this.GetDbConnection().QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandTimeout, commandType);
        }

        #endregion


    }
}
