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
    /// 角色功能
    /// </summary>
    public class RoleFunctionController : AdminBaseController
    {

        public RoleFunctionController()
        {
            this.MenuKey = "Z-140";
        }

        Sys_RoleMenuFunctionLogic _Logic = new Sys_RoleMenuFunctionLogic();

        #region 页面视图

        public IActionResult Index()
        {
            return View();
        }

        #endregion

        #region 获取列表数据源

        //public override PagingEntity GetPagingEntity(Hashtable query, int page = 1, int rows = 20)
        //{
        //    //获取列表
        //    return _Logic.GetDataSource(query, page, rows);
        //}

        #endregion

        #region 保存、删除

        [HttpPost, Aop.AopCheckEntityFilter]
        public async Task<IActionResult> Save(List<Sys_RoleMenuFunction> Sys_RoleMenuFunctionList, string RoleId)
        {
            await _Logic.SaveAsync(Sys_RoleMenuFunctionList, RoleId.ToGuid());
            return this.Success(new { status = 1 });
        }

        //[HttpPost]
        //public IActionResult Delete(string Ids)
        //{
        //    _Logic.Delete(Ids);
        //    return this.Success();
        //}

        #endregion

        #region 加载表单数据

        [HttpPost]
        public IActionResult LoadForm()
        {
            return this.Success(_Logic.LoadForm());
        }

        #endregion

        /// <summary>
        /// 获取角色菜单功能
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetRoleMenuFunctionTree(string RoleId)
        {
            return this.Success(new
            {
                status = 1,
                value = await _Logic.GetRoleMenuFunctionZTreeAsync(RoleId.ToGuid())
            });
        }

    }
}