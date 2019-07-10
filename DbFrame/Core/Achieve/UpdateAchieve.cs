﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DbFrame.Core.Achieve
{
    using DbFrame.BaseClass;
    using DbFrame.Core.Abstract;
    using DbFrame.Core.CodeAnalysis;
    using DbFrame.Core.Interface;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class UpdateAchieve<T> : AbstractUpdate<T>
    {
        public UpdateAchieve(MemberInitExpression _Set, AbstractAdo _Ado, Analysis _Analysis)
            : base(_Set, _Ado, _Analysis)
        {

        }

        public override int Execute()
        {
            this.ToSql();
            //如果开启了 Commit 状态
            if (Ado.CommitState)
                return Ado.Execute(this.Sql.Code.ToString(), this.Sql.GetDynamicParameters(), Ado._DbTransaction);
            return Ado.Execute(this.Sql.Code.ToString(), this.Sql.GetDynamicParameters());
        }

        public override Task<int> ExecuteAsync()
        {
            this.ToSql();
            //如果开启了 Commit 状态
            if (Ado.CommitState)
                return Task.FromResult(Ado.Execute(this.Sql.Code.ToString(), this.Sql.GetDynamicParameters(), Ado._DbTransaction));            
            return (Ado.ExecuteAsync(this.Sql.Code.ToString(), this.Sql.GetDynamicParameters()));
        }

        public override IUpdate<T> IgnoreCols(Expression<Func<T, dynamic>> IgnoreColumns)
        {
            this.IgnoreColumns.Add(this.IgnoreColsAnalysis(IgnoreColumns));

            return this;
        }

        public override IUpdate<T> IgnoreColsIF(bool IF, Expression<Func<T, dynamic>> IgnoreColumns)
        {
            if (IF) this.IgnoreCols(IgnoreColumns);

            return this;
        }

        public override IUpdate<T> Where(Expression<Func<HzyTuple<T>, bool>> Where)
        {
            if (Where == null) throw new DbFrameException("Where 条件不能为NULL!");

            this.Sql.Code_Where.Append(" AND ");

            this._Analysis.CreateWhere(Where, this.Sql);

            return this;
        }

        public override IUpdate<T> WhereIF(bool IF, Expression<Func<HzyTuple<T>, bool>> Where)
        {
            if (IF) this.Where(Where);

            return this;
        }

        public override SQL ToSql()
        {
            var _Table = _TableInfo.Item2;
            var _KeyInfo = _Table.FirstOrDefault(w => w.IsKey);
            var _TableName = _TableInfo.Item1;
            var TableName = DbSettings.KeywordHandle(_TableName);
            var _SetStr = new List<string>();
            //加入忽略字段 两个集合合并
            IgnoreColumns = IgnoreColumns.Union(_Table.Where(w => w.IsIgnore).Select(w => w.Name).ToList()).ToList();

            foreach (MemberAssignment item in memberInitExpression.Bindings)
            {
                var _Name = item.Member.Name;

                //检测有无忽略字段
                if (IgnoreColumns.Any(w => w == _Name)) continue;
                //如果主键自增
                if (_KeyInfo.Name == _Name && _KeyInfo.IsIdentity) continue;

                if (item.Expression is BinaryExpression)
                {
                    var _UpdateAnalysis = new UpdateAnalysis(this.Sql);
                    _UpdateAnalysis.Visit(item.Expression);
                    _SetStr.Add(_Name + "=" + _UpdateAnalysis._StringBuilder);
                }
                else
                {
                    var _Count = Sql.Parameter.Count;

                    var _Key = "@" + _Name + "_" + _Count;

                    _SetStr.Add(_Name + "=" + _Key);

                    var _Val = Parser.Eval(item.Expression);

                    Sql.Parameter.Add(new DbParam() { ParameterName = _Key, Value = _Val });
                }
            }

            this.Sql.Code.Append($"UPDATE {TableName} SET {string.Join(",", _SetStr)} WHERE 1=1 {this.Sql.Code_Where} ;");

            return this.Sql;
        }
    }

}