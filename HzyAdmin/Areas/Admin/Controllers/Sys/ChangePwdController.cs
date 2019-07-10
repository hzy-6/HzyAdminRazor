using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HzyAdmin.Areas.Admin.Controllers.Sys
{
    using System.Collections;
    using Entitys;
    using Entitys.SysClass;
    using Logic;
    using Logic.SysClass;
    using Toolkit;

    /// <summary>
    /// 修改密码
    /// </summary>
    public class ChangePwdController : AdminBaseController
    {
        public ChangePwdController()
        {
            this.MenuKey = "Z-150";
        }

        AccountLogic _Logic = new AccountLogic();

        #region 页面视图

        public IActionResult Index()
        {
            ViewBag.UserName = _Account.UserName;
            return View();
        }

        #endregion

        [HttpPost]
        public async Task<IActionResult> ChangePwd(string oldpwd, string newpwd, string newlypwd)
        {
            await _Logic.ChangePwd(oldpwd, newpwd, newlypwd);
            return this.Success();
        }


    }
}