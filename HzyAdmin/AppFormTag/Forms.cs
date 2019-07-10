using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AppFormTag
{
    using DbFrame.BaseClass;
    using Microsoft.AspNetCore.Html;
    using AppFormTag.Base;

    /// <summary>
    /// 表单基类
    /// </summary>
    public class FormsBase
    {
        /// <summary>
        /// 解析表达式
        /// </summary>
        /// <param name="Field"></param>
        /// <returns>(string,string) Item1=字段名 Item2=显示名称</returns>
        protected static (string, string) AnalysisExpression<T>(Expression<Func<T, object>> Field)
            where T : class, new()
        {
            var Name = string.Empty;
            var Title = string.Empty;

            var body = Field.Body;

            MemberExpression member = null;

            if (body is UnaryExpression)
            {
                var _UnaryExpression = body as UnaryExpression;
                member = _UnaryExpression.Operand as MemberExpression;
            }
            else if (body is ConstantExpression)
            {
                //var _ConstantExpression = body as ConstantExpression;

            }
            else if (body is MethodCallExpression)
            {
                //var _ConstantExpression = body as MethodCallExpression;
            }
            else if (body is MemberExpression)
            {
                member = body as MemberExpression;
            }

            if (member == null) throw new Exception("语法错误!");

            Name = member.Member.Name;
            var TableName = DbTable.GetTableName(typeof(T));
            Tuple<string, List<FieldDescribe>> Tuple = DbTable.GetTable(TableName);

            Title = Tuple.Item2.Find(w => w.Name == Name).Alias;

            return (Name, Title);
        }

        /// <summary>
        /// 将匿名对象转换为字典
        /// </summary>
        /// <param name="Attribute"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ObjectToDictionary(object Attribute)
        {
            var di = new Dictionary<string, string>();

            if (Attribute == null) return di;

            Type ty = Attribute.GetType();

            var fields = ty.GetProperties().ToList();

            foreach (var item in fields)
            {
                var Name = "";
                if (item.Name.Contains("_"))
                    Name = item.Name.Replace("_", "-");
                else
                    Name = item.Name;

                di.Add(Name, item.GetValue(Attribute).ToString());
            }

            return di;
        }

        public static HtmlString CreateHtml<T>(Expression<Func<T, object>> Field, Func<string, string, string> Bodys)
            where T : class, new()
        {
            var _Field = AnalysisExpression(Field);

            var _Html = Bodys.Invoke(_Field.Item1, _Field.Item2);

            return new HtmlString(_Html);
        }

        /// <summary>
        /// 获取字段显示名称
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Field"></param>
        /// <returns></returns>
        public static string GetDisplayName<T>(Expression<Func<T, object>> Field)
            where T : class, new()
        {
            var _Field = AnalysisExpression(Field);

            return _Field.Item2;
        }

        /// <summary>
        /// 获取字段名称和显示名称
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Field">字段</param>
        /// <returns>(string,string) Item1=字段名、Item2=显示名称</returns>
        public static (string,string) GetField<T>(Expression<Func<T, object>> Field)
            where T : class, new()
        {
            var _Field = AnalysisExpression(Field);

            return _Field;
        }


    }
}
