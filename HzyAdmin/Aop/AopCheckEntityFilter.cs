namespace Aop
{
    //
    using Microsoft.AspNetCore.Mvc.Filters;
    using Toolkit;
    using DbFrame;
    using DbFrame.DbContext.SqlServer;
    using Logic.Class;
    using Entitys.Class;

    /// <summary>
    /// 实体验证 特性
    /// </summary>
    public class AopCheckEntityFilter : ActionFilterAttribute
    {
        private DbContextSqlServer db = AppBase.db;
        private CheckModel<BaseClass> _CheckModel = new CheckModel<BaseClass>();
        public string[] ParamName { get; set; }

        public AopCheckEntityFilter()
        {
            this.ParamName = new string[] { "model" };
        }

        public AopCheckEntityFilter(string[] _ParamName)
        {
            this.ParamName = _ParamName;
        }

        /// <summary>
        /// 每次请求Action之前发生，，在行为方法执行前执行
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (this.ParamName != null)
            {
                foreach (var item in this.ParamName)
                {
                    var _ActionArguments = filterContext.ActionArguments;

                    if (!_ActionArguments.ContainsKey(item)) continue;
                    var _Value = (Entitys.Class.BaseClass)_ActionArguments[item];
                    if (_Value != null)
                    {
                        if (!_CheckModel.Check(_Value))
                        {
                            throw new MessageBox(_CheckModel.ErrorMessage);
                        }
                    }
                }
            }

        }

    }
}
