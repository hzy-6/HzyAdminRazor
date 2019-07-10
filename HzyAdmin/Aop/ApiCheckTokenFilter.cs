
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
    /// 检查 Authorization
    /// </summary>
    public class ApiCheckTokenFilter : ActionFilterAttribute
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

            if (!Ignore) return;

            if (!context.HttpContext.Request.Headers.ContainsKey("Authorization")) throw new MessageBox("接口未授权!");

            //得到 jwt 信息
            var _Controller = context.Controller as HzyAdmin.Controllers.Api.ApiBaseController;
            if (_Controller == null) throw new MessageBox("登录超时!", EMsgStatus.登录超时20);
            var _ClaimsIdentity = _Controller.User.Identity as ClaimsIdentity;

            var _Id = _ClaimsIdentity.Name;

            if (_Id.ToGuid() == Guid.Empty) throw new MessageBox("登录超时!", EMsgStatus.登录超时20);

            //这里对你 _Controller  里面的 用户变量赋值



        }

    }
}
