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
    /// 操作日志
    /// </summary>
    public class AppLogController : AdminBaseController
    {
        public AppLogController()
        {
            this.MenuKey = "Z-170";
        }

        Sys_AppLogLogic _Logic = new Sys_AppLogLogic();

        #region 页面视图

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Info()
        {
            return View();
        }

        #endregion

        #region 获取列表数据源

        public override PagingEntity GetPagingEntity(Hashtable query, int page = 1, int rows = 20)
        {
            //获取列表
            return _Logic.GetDataSource(query, page, rows);
        }

        #endregion

        #region 保存、删除

        //[HttpPost, Aop.AopCheckEntityFilter]
        //public IActionResult Save(Sys_Function model)
        //{
        //    this.FormKey = _Logic.Save(model);
        //    return this.Success(new { status = 1, formKey = this.FormKey });
        //}

        //[HttpPost]
        //public IActionResult Delete(string Ids)
        //{
        //    _Logic.Delete(Ids);
        //    return this.Success();
        //}

        #endregion

        #region 加载表单数据

        [HttpPost]
        public IActionResult LoadForm(string ID)
        {
            return this.Success(_Logic.LoadForm(ID.ToGuid()));
        }

        #endregion


    }
}