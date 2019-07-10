namespace DbFrame.Core.CodeAnalysis
{
    //
    using DbFrame.BaseClass;

    public class TopAnalysis
    {
        public void Create(SQL _Sql, int Top, Analysis analysis)
        {
            if (analysis._DbContextType == DbContextType.SqlServer)
            {
                var _Codes = _Sql.Code_Column.ToString().ToLower();
                if (!_Codes.Contains("top"))
                {
                    if (_Codes.Contains("DISTINCT"))
                    {
                        _Sql.Code_Column.Insert(0, " DISTINCT TOP (" + Top + ") ");
                    }
                    else
                    {
                        _Sql.Code_Column.Insert(0, " TOP (" + Top + ") ");
                    }
                }
            }
            else if (analysis._DbContextType == DbContextType.MySql)
            {
                var _Codes = _Sql.Code_Column.ToString().ToLower();
                _Sql.Code_TakePage.Insert(0, " LIMIT " + Top);
            }

        }



    }
}
