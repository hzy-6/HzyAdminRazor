using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aop
{
    using Entitys.SysClass;
    using Logic.SysClass;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System.Security.Claims;
    using Toolkit;

    /// <summary>
    /// 检查session 
    /// </summary>
    public class AdminActionFilter : ActionFilterAttribute
    {
        /// <summary>
        /// 忽略特性
        /// </summary>
        public bool Ignore { get; set; } = true;

        /// <summary>
        /// 每次请求Action之前发生，，在行为方法执行前执行
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            var _RouteValues = context.ActionDescriptor.RouteValues;
            var _AreaName = _RouteValues["area"];
            var _ControllerName = _RouteValues["controller"];
            var _ActionName = _RouteValues["action"];

            if (!Ignore) return;
            var _Controller = context.Controller as HzyAdmin.Areas.Admin.Controllers.AdminBaseController;

            var accountM = Tools.GetSession<Account>("Account");
            //如果没有忽略Session 检查
            if (!_Controller.IgnoreSessionCheck)
            {
                if (accountM == null || accountM?.UserID.ToGuid() == Guid.Empty)
                {
                    if (Tools.IsAjaxRequest)
                    {
                        context.Result = _Controller.Json(new ErrorModel(AppConfig.LoginPageUrl, EMsgStatus.登录超时20));
                    }
                    else
                    {
                        var Alert = $@"<script type='text/javascript'>
                                        alert('登录超时！系统将退出重新登录！');
                                        top.window.location='{AppConfig.LoginPageUrl}';
                                    </script>";
                        context.Result = _Controller.Content(Alert, "text/html;charset=utf-8;");
                    }
                }
                _Controller._Account = accountM;
            }

            AccountLogic.InsertAppLog(context.HttpContext, accountM.UserID);

            //如果表单key 存在 则自动将 控制器 基类 表单key 属性赋值上
            var _FormKey = _Controller.Request.Query["formKey"].ToStr();
            if (!string.IsNullOrWhiteSpace(_FormKey))
            {
                _Controller.ViewBag.FormKey = _FormKey;
                _Controller.FormKey = _FormKey;
            }

            //如果配置了 菜单 标识  则表示 要对他进行权限验证
            if (!string.IsNullOrWhiteSpace(_Controller.MenuKey) & !Tools.IsAjaxRequest)
            {

                if (!Tools.IsAjaxRequest)
                {
                    var db = _Controller.db;
                    var MenuKey = _Controller.MenuKey;

                    var _func_list = db.Query<Sys_Function>().OrderBy(w => w.t1.Function_Num).ToList();
                    var _power_list = new Dictionary<string, bool>();
                    //这里得判断一下是否是查找带回调用页面
                    string findback = context.HttpContext.Request.Query["findback"];

                    if (string.IsNullOrEmpty(findback))
                    {
                        //dynamic model = new ExpandoObject();
                        if (string.IsNullOrEmpty(MenuKey))
                        {
                            throw new MessageBox($"区域({_AreaName}),控制器({_ControllerName}):的程序中缺少菜单ID");
                        }

                        var _Menu = db.Find<Sys_Menu>(w => w.t1.Menu_Num == MenuKey);
                        if (!_Menu.Menu_Url.ToStr().StartsWith($"/{_AreaName}/{_ControllerName}/"))
                        {
                            throw new MessageBox($"区域({_AreaName}),控制器({_ControllerName}):的程序中缺少菜单ID与该页面不匹配");
                        }

                        var _role_menu_func_list = db.Query<Sys_RoleMenuFunction>().ToList();
                        var _menu_func_list = db.Query<Sys_MenuFunction>().ToList();

                        if (_Controller._Account.IsSuperManage)
                        {
                            _func_list.ForEach(item =>
                            {
                                var ispower = _menu_func_list.Any(w => w.MenuFunction_MenuID == _Menu.Menu_ID && w.MenuFunction_FunctionID == item.Function_ID);
                                if (item.Function_ByName == "Have" | _Menu.Menu_ParentID == AppConfig.SysMenuID) ispower = true;
                                _power_list.Add(item.Function_ByName, ispower);
                            });
                        }
                        else
                        {
                            _func_list.ForEach(item =>
                            {
                                if (_Controller._Account.RoleIDList == null)
                                {
                                    _power_list.Add(item.Function_ByName, false);
                                }
                                else
                                {
                                    var ispower = _role_menu_func_list.Any(w =>
                                        _Controller._Account.RoleIDList.Contains(w.RoleMenuFunction_RoleID.ToGuid()) &&
                                        w.RoleMenuFunction_MenuID == _Menu.Menu_ID &&
                                        w.RoleMenuFunction_FunctionID == item.Function_ID);
                                    _power_list.Add(item.Function_ByName, ispower);
                                }
                            });
                        }
                    }
                    else
                    {
                        _func_list.ForEach(item =>
                        {
                            _power_list.Add(item.Function_ByName, false);
                        });
                        _power_list["Have"] = true;
                        _power_list["Search"] = true;
                    }

                    _Controller.ViewData["PowerModel"] = _power_list.SerializeObject();
                    _Controller.ViewData["thisWindowName"] = $"adminIframe-{_Controller.HttpContext.Request.Path}{_Controller.HttpContext.Request.QueryString}";
                    _Controller.ViewData["formWindowName"] = $"Form_{_Controller.HttpContext.Request.Path.ToStr().Replace("/", "")}";
                }


            }


        }


    }
}
