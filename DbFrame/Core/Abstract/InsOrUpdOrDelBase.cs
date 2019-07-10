using System;
using System.Collections.Generic;
using System.Text;

namespace DbFrame.Core.Abstract
{
    using DbFrame.BaseClass;
    using DbFrame.Core.CodeAnalysis;
    using System.Linq.Expressions;

    public class InsOrUpdOrDelBase<T>
    {
        protected SQL Sql { get; set; }
        protected List<string> IgnoreColumns { get; set; }
        protected AbstractAdo Ado { get; set; }
        protected Analysis _Analysis { get; set; }
        protected Tuple<string, List<FieldDescribe>> _TableInfo { get; set; }

        public InsOrUpdOrDelBase(AbstractAdo _Ado, Analysis analysis)
        {
            this.Sql = new SQL();
            this.IgnoreColumns = new List<string>();
            this.Sql.IsAlias = false;
            this.Ado = _Ado;
            this._Analysis = analysis;
            this._TableInfo = DbTable.GetTable(typeof(T));
        }

        /// <summary>
        /// 忽略列解析
        /// </summary>
        /// <param name="IgnoreColumns"></param>
        /// <returns></returns>
        protected string IgnoreColsAnalysis(Expression<Func<T, dynamic>> IgnoreColumns)
        {
            var _Body = IgnoreColumns.Body;

            if (_Body is UnaryExpression)
            {
                var _UnaryExpression = _Body as UnaryExpression;
                var _Operand = _UnaryExpression.Operand;
                if (_Operand is MemberExpression)
                {
                    var _MemberExpression = _Operand as MemberExpression;
                    return _MemberExpression.Member.Name;
                }
            }
            else if (_Body is MemberExpression)
            {
                var _MemberExpression = _Body as MemberExpression;
                return _MemberExpression.Member.Name;
            }

            throw new DbFrameException("IgnoreCols 语法无法解析!");
        }



    }
}
