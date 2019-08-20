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
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Toolkit;

    /// <summary>
    /// 创建代码
    /// </summary>
    public class CreateCodeController : AdminBaseController
    {


        private IHostingEnvironment _IHostingEnvironment = null;
        private string _WebRootPath = string.Empty;
        public CreateCodeController(IHostingEnvironment IHostingEnvironment)
        {
            this._IHostingEnvironment = IHostingEnvironment;
            _WebRootPath = this._IHostingEnvironment.WebRootPath;
        }

        public Sys_CreateCodeLogic _Logic = new Sys_CreateCodeLogic();

        protected CreateCodeController()
        {
            this.MenuKey = "Z-160";
        }

        public IActionResult Index()
        {
            ViewData["DbSetCode"] = _Logic.CreateDbSetCode();
            ViewData["Path"] = (_WebRootPath + "\\Content\\CreateFile\\").Replace("\\", "\\\\");
            return View();
        }

        /// <summary>
        /// 获取数据库中所有的表和字段
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetZTreeAllTable()
        {
            return this.Success(new { status = 1, value = _Logic.GetZTreeAllTable() });
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Save(IFormCollection fc)
        {
            var Type = fc["ClassType"];
            var Url = (fc["Url"].ToStr() == null ? _WebRootPath + "\\Content\\CreateFile\\" : fc["Url"].ToStr());
            var Str = fc["Str"];
            var Table = fc["Table"];
            var isall = fc["isall"].ToBool();
            var Template = _WebRootPath + "\\Content\\Template\\";

            _Logic.Save(Type, Url, Template, Str, isall, Table).ConfigureAwait(false);
            return this.Success();
        }

















    }
}