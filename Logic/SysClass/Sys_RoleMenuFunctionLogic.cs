using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.SysClass
{
    using System.Data;
    using System.Collections;
    using Toolkit;
    using Logic.Class;
    using Entitys.SysClass;
    using DbFrame;

    public class Sys_RoleMenuFunctionLogic : BaseLogic
    {

        #region  增、删、改、查

        /// <summary>
        /// 保存角色功能树
        /// </summary>
        /// <param name="Sys_RoleMenuFunction_List"></param>
        /// <param name="RoleId"></param>
        public async Task SaveAsync(List<Sys_RoleMenuFunction> Sys_RoleMenuFunction_List, Guid RoleId)
        {
            await db.CommitAsync(async () =>
            {
                await db.DeleteAsync<Sys_RoleMenuFunction>(w => w.t1.RoleMenuFunction_RoleID == RoleId);
                foreach (var item in Sys_RoleMenuFunction_List)
                {
                    await db.InsertAsync(item);
                }
            });
        }

        /// <summary>
        /// 表单数据加载
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Hashtable LoadForm()
        {
            var _Sys_RoleList = db.Query<Sys_Role>().OrderBy(w => w.SqlStr($"convert(int,{nameof(w.t1.Role_Num)})")).ToList<Dictionary<string, object>>();

            foreach (var item in _Sys_RoleList)
            {
                item["State"] = _Sys_RoleList.IndexOf(item) == 0 ? 1 : 0;
            }

            var _Form = ObjectToHashtable(new
            {
                status = 1,
                Sys_RoleList = _Sys_RoleList
            });

            return _Form;
        }
        #endregion

        /// <summary>
        /// 获取角色菜单功能树
        /// </summary>
        /// <param name="RoleId"></param>
        /// <returns></returns>
        public async Task<List<object>> GetRoleMenuFunctionZTreeAsync(Guid RoleId)
        {
            var list = new List<object>();
            var _Sys_Menu_List = await db.Query<Sys_Menu>().OrderBy(w => w.t1.Menu_Num).ToListAsync();
            var _Sys_Function_List = await db.Query<Sys_Function>().OrderBy(w => w.SqlStr($"convert(int,{nameof(w.t1.Function_Num)})")).ToListAsync();
            var _Sys_MenuFunction_List = await db.Query<Sys_MenuFunction>().OrderBy(w => w.t1.MenuFunction_CreateTime).ToListAsync();
            var _Sys_RoleMenuFunction = await db.Query<Sys_RoleMenuFunction>().ToListAsync();

            foreach (var item in _Sys_Menu_List)
            {
                list.Add(new
                {
                    id = item.Menu_ID,
                    name = item.Menu_Name + "(" + item.Menu_Num + ")",
                    pId = item.Menu_ParentID,
                    tag = "Menu",
                    @checked = false
                });
                //判断是否为末级菜单
                if (!_Sys_Menu_List.Any(w => w.Menu_ParentID == item.Menu_ID))
                {
                    //遍历 菜单拥有的功能
                    var _SysMenuFunctionList = _Sys_MenuFunction_List.FindAll(w => w.MenuFunction_MenuID == item.Menu_ID);
                    foreach (var _MenuFunction in _SysMenuFunctionList)
                    {
                        //得到功能信息
                        var _Function = _Sys_Function_List.Find(w => w.Function_ID == _MenuFunction.MenuFunction_FunctionID);

                        //判断该角色 对应的菜单和功能是否存在
                        var _Any = _Sys_RoleMenuFunction.Any(w =>
                          w.RoleMenuFunction_RoleID == RoleId &&
                          w.RoleMenuFunction_MenuID == item.Menu_ID &&
                          w.RoleMenuFunction_FunctionID == _MenuFunction.MenuFunction_FunctionID);

                        list.Add(new
                        {
                            id = _MenuFunction.MenuFunction_FunctionID,
                            name = _Function.Function_Name,
                            pId = item.Menu_ID,
                            tag = "MenuFunction",
                            @checked = _Any
                        });
                    }
                }
            }
            return list;
        }





    }
}
