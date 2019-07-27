using System;
using System.Collections.Generic;
using System.Text;

namespace DbFrame.Core.Abstract.Query
{
    using DbFrame.BaseClass;
    using DbFrame.Core.Interface.Query;
    using System.Data;
    using System.Threading.Tasks;

    public abstract class AbstractMappingData<T> : AbstractBase, IMappingData<T>
    {
        public abstract T Frist();
        public abstract List<T> ToList();

        public abstract TReturn Frist<TReturn>();
        public abstract List<TReturn> ToList<TReturn>();
        public abstract DataTable ToTable();
        public abstract int Count();
        public abstract List<DbParam> GetDbParam();
        public abstract SQL ToSql();
        //
        public abstract IMappingData Top(int Top);
        public abstract IMappingData Distinct();
        public abstract IMappingData TakePage(int PageNumber, int PageSize);
        public abstract IMappingData TakePage(int PageNumber, int PageSize, out int TotalCount);
        public abstract IMappingData ToSql(out SQL _SQL);

        #region Async
        public abstract Task<T> FristAsync();
        public abstract Task<List<T>> ToListAsync();
        public abstract Task<TReturn> FristAsync<TReturn>();
        public abstract Task<List<TReturn>> ToListAsync<TReturn>();
        public abstract Task<DataTable> ToTableAsync();
        public abstract Task<int> CountAsync();
        #endregion Async

    }
}
