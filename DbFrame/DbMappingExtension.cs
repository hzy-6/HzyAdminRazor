using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace DbFrame
{
    //
    using System.Linq.Expressions;
    using System.Reflection;
    using DbFrame.BaseClass;
    using Dapper;

    public static class DbMappingExtension
    {
        /// <summary>
        /// DataRow 转换 实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static T ToEntity<T>(this DataRow dr) where T : class, new()
        {
            var _Entity = Parser.CreateInstance<T>();
            var list = _Entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            if (list.Length == 0) throw new Exception("找不到任何 公共属性！");

            foreach (var item in list)
            {
                string AttrName = item.Name;
                foreach (DataColumn dc in dr.Table.Columns)
                {
                    if (AttrName != dc.ColumnName) continue;
                    if (dr[dc.ColumnName] != DBNull.Value) item.SetValue(_Entity, dr[dc.ColumnName], null);
                }
            }
            return _Entity;
        }

        /// <summary>
        /// 将 datatable 转换为 list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this DataTable table) where T : class, new()
        {
            var list = new List<T>();

            var _Entity = Parser.CreateInstance<T>();
            var propertyInfo = _Entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (DataRow dr in table.Rows)
            {
                _Entity = Parser.CreateInstance<T>();
                foreach (var item in propertyInfo)
                {
                    string AttrName = item.Name;
                    foreach (DataColumn dc in dr.Table.Columns)
                    {
                        if (AttrName != dc.ColumnName) continue;
                        if (dr[dc.ColumnName] != DBNull.Value)
                            item.SetValue(_Entity, dr[dc.ColumnName], null);
                        else
                            item.SetValue(_Entity, null, null);
                    }
                }
                list.Add(_Entity);
            }
            return list;
        }

        /// <summary>
        /// datatable 转换为 List<Dictionary<string,object>>
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> ToList(this DataTable table)
        {
            var list = new List<Dictionary<string, object>>();
            var dic = new Dictionary<string, object>();
            foreach (DataRow dr in table.Rows)
            {
                if (dic != null) dic = new Dictionary<string, object>();
                foreach (DataColumn dc in table.Columns)
                {
                    if (dr[dc.ColumnName] == DBNull.Value)
                    {
                        dic.Add(dc.ColumnName, null);
                    }
                    else
                    {
                        if (dc.DataType == typeof(DateTime))
                            dic.Add(dc.ColumnName, Convert.ToDateTime(dr[dc.ColumnName]).ToString("yyyy-MM-dd HH:mm:ss"));
                        else
                            dic.Add(dc.ColumnName, dr[dc.ColumnName]);
                    }
                }
                list.Add(dic);
            }
            return list;
        }

        /// <summary>
        /// IDataReader 转换为 DataTable
        /// </summary>
        /// <param name="_IDataReader"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(this IDataReader _IDataReader)
        {
            DataTable dt = new DataTable();
            dt.Load(_IDataReader);
            return dt;
        }

        /// <summary>
        /// 将匿名对象转换为字典
        /// </summary>
        /// <param name="Attribute"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDictionary<T>(this T Attribute) where T : class, new()
        {
            var di = new Dictionary<string, object>();

            Type ty = Attribute.GetType();

            var fields = ty.GetProperties().ToList();

            foreach (var item in fields) di.Add(item.Name, item.GetValue(Attribute).ToString());

            return di;
        }

        /// <summary>
        /// 两实体映射
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="M"></typeparam>
        /// <param name="Entity"></param>
        /// <param name="Model"></param>
        /// <returns></returns>
        public static M ToNewEntity<T, M>(this T Entity, M Model) where T : class, new()
        {
            //old
            var oldList = Entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var newList = Model.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var oldM in oldList)
            {
                foreach (var newM in newList)
                {
                    if (oldM.Name != newM.Name) continue;

                    newM.SetValue(Model, oldM.GetValue(Entity));
                }
            }

            return Model;
        }

        /************sql****************/
        /// <summary>
        /// 在 拉姆达表达式 where 表达式中使用 w => w.In(w.t1.Member_ID, guidsArray)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        public static bool In<T, T1>(this HzyTuple obj, T1 field, List<T> array)
        {
            return true;
        }

        /// <summary>
        /// 在 拉姆达表达式 where 表达式中使用 w => w.NotIn(w.t1.Member_ID, guidsArray)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        public static bool NotIn<T, T1>(this HzyTuple obj, T1 field, List<T> array)
        {
            return true;
        }

        /// <summary>
        /// 一般在Where 条件中使用 例如 : w.SqlStr("convert(varchar(50),UserName,23)")
        /// 
        /// 一般用来支持这种语法》CONVERT(varchar(100), GETDATE(), 23)    -- 2006-05-16 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SqlStr(this HzyTuple obj, string value)
        {
            return true;
        }

        /// <summary>
        /// 一般在Where 条件中使用 例如 : w.SqlStr("convert(varchar(50),UserName,23)")
        /// 
        /// 一般用来支持这种语法》CONVERT(varchar(100), GETDATE(), 23)    -- 2006-05-16 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TReturn SqlStr<TReturn>(this HzyTuple obj, string value)
        {
            return default(TReturn);
        }

    }



    public class Parser
    {
        /// <summary>
        /// 将 Model 转换为 MemberInitExpression 类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Model"></param>
        /// <returns></returns>
        public static MemberInitExpression ModelToMemberInitExpression<T>(T Model)
        {
            var proInfo = Parser.GetPropertyInfos(typeof(T));

            var list = new List<MemberBinding>();

            foreach (var item in proInfo) list.Add(Expression.Bind(item, Expression.Constant(item.GetValue(Model), item.PropertyType)));

            var newExpr = Expression.New(typeof(T));

            return Expression.MemberInit(newExpr, list);
        }

        /// <summary>
        /// 获取 PropertyInfo 集合
        /// </summary>
        /// <param name="_type"></param>
        /// <param name="_bindingFlags"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetPropertyInfos(Type _type, BindingFlags _bindingFlags = (BindingFlags.Instance | BindingFlags.Public))
        {
            return _type.GetProperties(_bindingFlags);
        }

        /// <summary>
        /// 创建 对象实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateInstance<T>()
        {
            var _Type = typeof(T);
            if (_Type.IsValueType || typeof(T) == typeof(string))
                return default(T);
            return (T)Activator.CreateInstance(_Type);
        }

        /// <summary>
        /// 获取 对象 中 某个属性得 标记
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_type"></param>
        /// <param name="_name"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(Type _type, string _name) where T : Attribute
        {
            return Parser.GetPropertyInfo(_type, _name).GetCustomAttribute(typeof(T)) as T;
        }

        /// <summary>
        /// 获取 PropertyInfo 对象
        /// </summary>
        /// <param name="_type"></param>
        /// <param name="_name"></param>
        /// <returns></returns>
        public static PropertyInfo GetPropertyInfo(Type _type, string _name)
        {
            return _type.GetProperty(_name);
        }

        /// <summary>
        /// 获取 TableAttribute
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        public static TableAttribute GetTableAttribute(Type _type)
        {
            return (TableAttribute)Attribute.GetCustomAttributes(_type, true).Where(item => item is TableAttribute).FirstOrDefault();
        }

        /// <summary>
        /// Eval
        /// </summary>
        /// <param name="_Expression"></param>
        /// <returns></returns>
        public static object Eval(Expression _Expression)
        {
            var cast = Expression.Convert(_Expression, typeof(object));
            return Expression.Lambda<Func<object>>(cast).Compile().Invoke();
        }

        /// <summary>
        /// 获取 DynamicParameters 对象
        /// </summary>
        /// <param name="dbParams"></param>
        /// <returns></returns>
        public static DynamicParameters GetDynamicParameters(List<DbParam> dbParams)
        {
            var _DynamicParameters = new DynamicParameters();
            foreach (var item in dbParams) _DynamicParameters.Add(item.ParameterName, item.Value);
            return _DynamicParameters;
        }

        /// <summary>
        /// 根据实体对象 的 ID 创建 Expression<Func<T, bool>> 表达式树
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_KeyName"></param>
        /// <param name="_KeyValue"></param>
        /// <returns></returns>
        public static Expression<Func<HzyTuple<T>, bool>> WhereById<T>(string _KeyName, object _KeyValue, string _ParName = "_Where_Parameter")
        {
            //创建 Where Lambda表达式树
            var _Type = typeof(HzyTuple<T>);
            var _Parmeter = Expression.Parameter(_Type, "w");
            var _Where_Parameter = Expression.Parameter(_Type, _ParName);
            var _Propertie = _Type.GetProperties()[0];
            var _Property = _Type.GetProperty(_Propertie.Name);

            var _Left = Expression.Property(_Parmeter, _Property);

            var _KeyProperty = _Property.PropertyType.GetProperty(_KeyName);

            var _NewLeft = Expression.Property(_Left, _KeyProperty);

            if (_KeyValue == null)
            {
                if (_KeyProperty.PropertyType == typeof(Guid)) _KeyValue = Guid.Empty;

                if (_KeyProperty.PropertyType == typeof(int)) _KeyValue = Int32.MinValue;
            }
            ConstantExpression _Right = Expression.Constant(_KeyValue);
            try
            {
                if (_KeyProperty.PropertyType == typeof(Guid)) _Right = Expression.Constant(_KeyValue, typeof(Guid));

                if (_KeyProperty.PropertyType == typeof(int)) _Right = Expression.Constant(_KeyValue, typeof(int));

                if (_KeyProperty.PropertyType == typeof(Guid?)) _Right = Expression.Constant(_KeyValue, typeof(Guid?));

                if (_KeyProperty.PropertyType == typeof(int?)) _Right = Expression.Constant(_KeyValue, typeof(int?));
            }
            catch (Exception ex)
            {
                if (_KeyProperty.PropertyType != _KeyValue.GetType())
                    throw new DbFrameException("请将主键值 转换为 正确的类型值！");
                else
                    throw ex;
            }

            var _Where_Body = Expression.Equal(_NewLeft, _Right);
            return Expression.Lambda<Func<HzyTuple<T>, bool>>(_Where_Body, _Where_Parameter);
        }


    }


    public class DbSettings
    {

        /// <summary>
        /// 默认连接字符串
        /// </summary>
        public static string DefaultConnectionString { get; set; }

        /// <summary>
        /// 关键字处理 函数
        /// </summary>
        public static Func<string, string> KeywordHandle;

    }

    /// <summary>
    /// 多表连接类型
    /// </summary>
    public enum EJoinType
    {
        /// <summary>
        /// join
        /// </summary>
        JOIN,

        /// <summary>
        /// 内连接 inner join
        /// </summary>
        INNER_JOIN,

        /// <summary>
        /// 左连接 left join
        /// </summary>
        LEFT_JOIN,

        /// <summary>
        /// 左外连接 left outer join
        /// </summary>
        LEFT_OUTER_JOIN,

        /// <summary>
        /// 右连接 right join
        /// </summary>
        RIGHT_JOIN,

        /// <summary>
        /// 右外连接 right outer join
        /// </summary>
        RIGHT_OUTER_JOIN,

        /// <summary>
        /// 全连接 full join
        /// </summary>
        FULL_JOIN,

        /// <summary>
        /// 全外连接 full outer join
        /// </summary>
        FULL_OUTER_JOIN,

        /// <summary>
        /// 交叉连接 cross join
        /// </summary>
        CROSS_JOIN
    }

    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum DbContextType
    {
        SqlServer,
        MySql,
        Oracle,
        PostgreSQL
    }


}
