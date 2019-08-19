using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Class
{
    using System.Data;
    using System.Data.Common;
    using Entitys.SysClass;
    using Toolkit;
    using DbFrame;
    using DbFrame.BaseClass;
    using Dapper;
    using DbFrame.Core.Interface.Query;

    public class BaseLogic : AppBase
    {
        /// <summary>
        /// 登录 信息 对象
        /// </summary>
        protected Account _Account => BaseLogic.GetAccount();

        public BaseLogic() { }

        public void SetSession(string key, object value)
        {
            Tools.SetSession(key, value);
        }

        public TResult GetSession<TResult>(string key)
        {
            return Tools.GetSession<TResult>(key);
        }

        public static Account GetAccount()
        {
            return new SysClass.AccountLogic().Get().Result;
        }

        /// <summary>
        /// 分页查询 通过 存储过程 PROC_SPLITPAGE
        /// </summary>
        /// <param name="SqlStr"></param>
        /// <param name="Page"></param>
        /// <param name="Rows"></param>
        /// <param name="Param"></param>
        /// <param name="TableNames">如果使用后端生成表头，则将查询所用到的表对应的Model传入进来</param>
        /// <param name="CallBack">数据完成后回调</param>
        /// <returns></returns>
        public PagingEntity GetPagingEntity(string SqlStr, int Page, int Rows, DbParameter[] Param, List<string> TableNames, Action<PagingEntity> CallBack = null)
        {
            var _PagingEntity = new PagingEntity();

            if (Param != null)
            {
                //解析参数
                foreach (var item in Param)
                {
                    SqlStr = SqlStr.Replace("@" + item.ParameterName, item.Value == null ? null : "'" + item.Value + "' ");
                }
            }

            var _DynamicParameters = new DynamicParameters();
            _DynamicParameters.Add("@SQL", SqlStr, DbType.String, ParameterDirection.Input);
            _DynamicParameters.Add("@PAGE", Page, DbType.Int32, ParameterDirection.Input);
            _DynamicParameters.Add("@PAGESIZE", Rows, DbType.Int32, ParameterDirection.Input);
            _DynamicParameters.Add("@PAGECOUNT", 0, DbType.Int32, ParameterDirection.Output);
            _DynamicParameters.Add("@RECORDCOUNT", 0, DbType.Int32, ParameterDirection.Output);

            //将 IDataReader 对象转换为 DataSet 
            DataSet _DataSet = new AdoExtend.HZYDataSet();
            db.ExecuteReader("PROC_SPLITPAGE", _DynamicParameters, null, 30, CommandType.StoredProcedure, (_IDataReader) =>
            {
                _DataSet.Load(_IDataReader, LoadOption.OverwriteChanges, null, new DataTable[] { });
            });

            if (_DataSet.Tables.Count == 2)
            {
                var _Table = _DataSet.Tables[1];
                var _Total = _DynamicParameters.Get<int>("@RECORDCOUNT");
                _PagingEntity.Table = _Table;
                _PagingEntity.Counts = _Total;
                _PagingEntity.PageCount = (_Total / Rows);
                _PagingEntity.List = _Table.ToList();

                this.SetHeaderJson(_PagingEntity, TableNames.ToList());
            }

            CallBack?.Invoke(_PagingEntity);

            return _PagingEntity;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="Iquery">查询对象</param>
        /// <param name="_TotalCount">总数</param>
        /// <param name="Rows">每页显示条数</param>
        /// <param name="CustomHeader">是否采用后端生成表头</param>
        /// <param name="CallBack">数据完成后回调</param>
        /// <returns></returns>
        public PagingEntity GetPagingEntity(IMappingData Iquery, int _TotalCount, int Rows, bool CustomHeader = true, Action<PagingEntity> CallBack = null)
        {
            var _PagingEntity = new PagingEntity();

            var _Table = Iquery.ToTable();

            if (_Table.Columns.Contains("ROWID")) _Table.Columns.RemoveAt(0);

            _PagingEntity.Table = _Table;
            _PagingEntity.Counts = _TotalCount;
            _PagingEntity.PageCount = (_TotalCount / Rows);
            _PagingEntity.List = _Table.ToList();

            if (CustomHeader)
            {
                var _Sql = Iquery.ToSql();
                this.SetHeaderJson(_PagingEntity, _Sql.TableNames);
            }

            CallBack?.Invoke(_PagingEntity);

            return _PagingEntity;
        }

        /// <summary>
        /// 生产表头 Json 对象
        /// </summary>
        /// <param name="_PagingEntity"></param>
        /// <param name="ArryEntity"></param>
        private void SetHeaderJson(PagingEntity _PagingEntity, List<string> TableNames)
        {
            if (TableNames == null) return;
            if (TableNames.Count == 0) return;

            var _FieldDescribeList = new List<FieldDescribe>();
            foreach (var item in TableNames)
            {
                var Tuple = DbTable.GetTable(item);
                foreach (var Field in Tuple.Item2)
                {
                    _FieldDescribeList.Add(Field);
                }
            }

            foreach (DataColumn dc in _PagingEntity.Table.Columns)
            {
                var _Field = new Dictionary<string, object>();
                var _Column = new Dictionary<string, string>();
                var _FieldDescribe = _FieldDescribeList.Find(item => item.Name.Equals(dc.ColumnName));

                var field = dc.ColumnName;
                var title = dc.ColumnName == "_ukid" ? "ID" : dc.ColumnName;
                var align = "left";
                var sortable = dc.ColumnName != "_ukid";
                var visible = dc.ColumnName != "_ukid";

                if (_FieldDescribe != null) title = string.IsNullOrWhiteSpace(_FieldDescribe.Alias) ? title : _FieldDescribe.Alias;

                _Column.Add(field, title);
                _Field.Add(nameof(field), field);
                _Field.Add(nameof(align), align);
                _Field.Add(nameof(sortable), sortable);
                _Field.Add(nameof(title), title);
                _Field.Add(nameof(visible), visible);

                _PagingEntity.ColNames.Add(_Column);
                _PagingEntity.ColModel.Add(_Field);
            }
        }


    }
}
