using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HzyAdmin.Areas.Admin.Controllers
{
    using DbFrame.DbContext.SqlServer;
    using Entitys;
    using Entitys.SysClass;
    using Logic;
    using Logic.SysClass;
    using Microsoft.Extensions.Configuration;
    using Toolkit;

    public class LoginController : AdminBaseController
    {
        public LoginController()
        {
            this.IgnoreSessionCheck = true;
        }
        public AccountLogic _Logic = new AccountLogic();

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 检查帐户
        /// </summary>
        /// <param name="UserName">登陆名</param>
        /// <param name="UserPassword">登陆密码</param>
        /// <param name="LoginCode">验证码</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Check(string UserName, string UserPassword, string LoginCode)
        {
            var token = await _Logic.CheckedAsync(UserName, UserPassword, LoginCode);
            return this.Success(new
            {
                status = 1,
                jumpurl = AppConfig.HomePageUrl + "#!%u9996%u9875#!/Admin/Home/Main/",
                token = token
            });
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        public IActionResult Out()
        {
            Tools.RemoveCookie("Authorization");
            return RedirectToAction("Index");
        }









    }
}