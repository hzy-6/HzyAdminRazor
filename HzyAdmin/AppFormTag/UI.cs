using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppFormTag
{
    using AppFormTag.Base;
    using DbFrame;
    using Microsoft.AspNetCore.Html;
    using System.Linq.Expressions;
    using System.Text;

    public class UI : FormsBase
    {

        /// <summary>
        /// 创建 JS 实体
        /// </summary>
        /// <returns></returns>
        public static HtmlString CreateViewModel(params object[] Models)
        {
            var _StringBuilder = new StringBuilder();

            foreach (var item in Models)
            {
                if (item is string)
                    _StringBuilder.Append($"{item}:'',");
                else
                    Parser.GetPropertyInfos(item.GetType()).ToList().ForEach(x => _StringBuilder.Append($"{x.Name}:'',"));
            }

            return new HtmlString(_StringBuilder.ToString());
        }

        public static HtmlString Input<T>(Expression<Func<T, object>> Field, int Col = 6, object Attr = null)
            where T : class, new()
        {
            return CreateHtml<T>(Field, (name, title) =>
            {
                var dic = new Dictionary<string, string>();
                dic["type"] = "text";
                dic["class"] = "form-control";
                dic["placeholder"] = $"请输入 {title}";
                dic["v-model"] = $"{name}";
                dic["name"] = $"{name}";
                dic["autocomplete"] = "off";

                var _Attr = ObjectToDictionary(Attr);
                foreach (var item in _Attr)
                {
                    dic[item.Key] = item.Value;
                }

                var _Tmp = $@"<div class='col-sm-{Col}'>
                                <h4 class='example-title'>{title}</h4>
                                {PageControl.Input(dic)}
                            </div>";
                return _Tmp;
            });
        }

        public static HtmlString Select<T>(Expression<Func<T, object>> Field, Func<string> FuncOpetion, int Col = 6, object Attr = null)
            where T : class, new()
        {

            return CreateHtml<T>(Field, (name, title) =>
            {
                var dic = new Dictionary<string, string>();
                dic["class"] = "form-control";
                dic["v-model"] = $"{name}";
                dic["name"] = $"{name}";

                var _Attr = ObjectToDictionary(Attr);
                foreach (var item in _Attr)
                {
                    dic[item.Key] = item.Value;
                }

                var options = $"<option value=''>==请选择 {title}==</option>";

                options += FuncOpetion.Invoke();

                var _Tmp = $@"<div class='col-sm-{Col}'>
                                <h4 class='example-title'>{title}</h4>
                                { PageControl.Select(dic, options)}
                            </div>";
                return _Tmp;
            });
        }

        public static HtmlString Textarea<T>(Expression<Func<T, object>> Field, int Col = 6, object Attr = null)
            where T : class, new()
        {
            return CreateHtml<T>(Field, (name, title) =>
            {
                var dic = new Dictionary<string, string>();
                dic["class"] = "form-control";
                dic["placeholder"] = $"请输入 {title}";
                dic["v-model"] = $"{name}";
                dic["name"] = $"{name}";
                dic["autocomplete"] = "off";

                var _Attr = ObjectToDictionary(Attr);
                foreach (var item in _Attr)
                {
                    dic[item.Key] = item.Value;
                }

                var _Tmp = $@"<div class='col-sm-{Col}'>
                                <h4 class='example-title'>{title}</h4>
                                {PageControl.Textarea(dic)}
                            </div>";
                return _Tmp;
            });
        }

        public static HtmlString UEditor<T>(Expression<Func<T, object>> Field, int Col = 6, object Attr = null, string Width = "100%", string Height = "300px")
            where T : class, new()
        {
            return CreateHtml<T>(Field, (name, title) =>
            {
                var dic = new Dictionary<string, string>();
                dic["id"] = $"{name}";
                dic["name"] = $"{name}";
                dic["style"] = $"width:{Width};height:{Height}";
                dic["type"] = $"text/plain";

                var _Attr = ObjectToDictionary(Attr);
                foreach (var item in _Attr)
                {
                    dic[item.Key] = item.Value;
                }

                var _Tmp = $@"<div class='col-sm-{Col}'>
                                <h4 class='example-title'>{title}</h4>
                                {PageControl.Script(dic, "")}
                            </div>";
                return _Tmp;
            });
        }


        public static HtmlString FindBack<T, T1>(Expression<Func<T, object>> Text, Expression<Func<T1, object>> ID, string Url, string FindClick, string RemoveClick, int Col = 6, string Title = null, string Placeholder = null, bool KO = true, bool Readonly = true, string W = "1200px", string H = "1200px")
            where T : class, new()
            where T1 : class, new()
        {
            var Obj = AnalysisExpression(Text);
            var Obj1 = AnalysisExpression(ID);

            if (string.IsNullOrEmpty(Title))
                Title = Obj.Item2;

            var _New_Placeholder = (string.IsNullOrEmpty(Placeholder) ? "请选择 " + Title : Placeholder);

            return new HtmlString(FindBack(Title, Obj.Item1, Obj1.Item1, Url, FindClick, RemoveClick, Col, _New_Placeholder, Readonly, W, H));
        }

        public static string FindBack(string Title, string Text, string ID, string Url, string FindClick, string RemoveClick, int Col = 6, string Placeholder = null, bool Readonly = true, string W = "1200px", string H = "1200px")
        {
            var _Tmp = $@"<div class='col-sm-{Col}'>
            <h4 class='example-title'>{Title}</h4>
            <div class='input-group'>
                <input type='text' class='form-control' name='{Text}' v-model='{Text}' placeholder='{Placeholder}' {(Readonly ? "readonly='readonly'" : "")} />
                <input type='text' class='form-control hidden-xs-up' name='{ID}' v-model='{ID}' />
                <span class='input-group-btn'>" +
                      $"<button type='button' class='btn btn-outline btn-default' onclick=\"admin.findBack.open('{Url}', '{Placeholder}', function(row){{{FindClick}}},'{W}','{H}');\"><i class='fas fa-search'></i></button>" +
                      $"<button type='button' class='btn btn-outline btn-default' onclick=\"{RemoveClick}\"><i class='fas fa-times'></i></button>" +
                @"</span>
            </div>
        </div>";

            return _Tmp;
        }






    }
}
