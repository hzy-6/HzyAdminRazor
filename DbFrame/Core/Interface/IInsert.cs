using System;
using System.Collections.Generic;
using System.Text;

namespace DbFrame.Core.Interface
{
    using DbFrame.BaseClass;
    using System.Data;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IInsert<T>
    {
        object Execute();
        Task<object> ExecuteAsync();
        IInsert<T> IgnoreCols(Expression<Func<T, dynamic>> IgnoreColumns);
        IInsert<T> IgnoreColsIF(bool IF, Expression<Func<T, dynamic>> IgnoreColumns);
        SQL ToSql(Action<object> CallBack);
    }


}
