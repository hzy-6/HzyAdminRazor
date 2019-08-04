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

    public class Sys_MenuLogic : BaseLogic
    {
        #region  增、删、改、查

        /// <summary>
        /// 数据源
        /// </summary>
        /// <param name="Query"></param>
        /// <param name="Page"></param>
        /// <param name="Rows"></param>
        /// <returns></returns>
        public PagingEntity GetDataSource(Hashtable Query, int Page, int Rows)
        {
            var _Query = db
                .Query<Sys_Menu>()
                .Join<Sys_Menu>(w => w.t1.Menu_ParentID == w.t2.Menu_ID)
                .WhereIF(string.IsNullOrEmpty(Query["Menu_ID"].ToStr()), w => w.t1.Menu_ParentID == null)
                .WhereIF(!string.IsNullOrEmpty(Query["Menu_ID"].ToStr()), w => w.t1.Menu_ParentID == Query["Menu_ID"].ToGuid())
                .WhereIF(!string.IsNullOrEmpty(Query["Menu_Name"].ToStr()), w => w.t1.Menu_Name.Contains(Query["Menu_Name"].ToStr()));

            if (string.IsNullOrEmpty(Query["sortName"].ToStr()))
                _Query.OrderBy(w => new { w.t1.Menu_Num });
            else
                _Query.OrderBy(w => Query["sortName"].ToStr() + " " + Query["sortOrder"].ToStr());//前端自动排序

            var IQuery = _Query.Select(w => new
            {
                w.t1.Menu_Name,
                w.t1.Menu_Url,
                父级菜单 = w.t2.Menu_Name,
                w.t1.Menu_Num,
                w.t1.Menu_Icon,
                SqlString = $"case when t1.{nameof(w.t1.Menu_IsShow)}=2 then '隐藏' else '显示' end {nameof(w.t1.Menu_IsShow)}",
                w.t1.Menu_CreateTime,
                _ukid = w.t1.Menu_ID
            });

            IQuery.TakePage(Page, Rows, out int TotalCount);

            return this.GetPagingEntity(IQuery, TotalCount, Rows);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="_Sys_Function_List"></param>
        /// <returns></returns>
        public string Save(Sys_Menu model, List<Sys_MenuFunction> _Sys_MenuFunctionList)
        {
            model.Menu_IsShow = 1;

            db.Commit(() =>
            {
                if (model.Menu_ID.ToGuid() == Guid.Empty)
                {
                    model.Menu_ID = db.Insert(model).ToGuid();
                    if (model.Menu_ID.ToGuid().Equals(Guid.Empty)) throw new MessageBox(this.ErrorMessage);
                }
                else
                {
                    if (!db.UpdateById(model)) throw new MessageBox(this.ErrorMessage);
                }

                if (_Sys_MenuFunctionList.Count > 0)
                {
                    //删除菜单的功能
                    db.Delete<Sys_MenuFunction>(w => w.t1.MenuFunction_MenuID == model.Menu_ID.ToGuid());
                    _Sys_MenuFunctionList.ForEach(item =>
                    {
                        item.MenuFunction_MenuID = model.Menu_ID;
                        db.Insert(item);
                    });
                }

            });
            return model.Menu_ID.ToGuidStr();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="Ids"></param>
        public void Delete(string Ids)
        {
            db.Commit(() =>
            {
                Ids.DeserializeObject<List<Guid>>().ForEach(item =>
                {
                    //删除菜单的功能
                    db.Delete<Sys_MenuFunction>(w => w.t1.MenuFunction_MenuID == item);
                    db.DeleteById<Sys_Menu>(item);
                });
            });
        }

        /// <summary>
        /// 表单数据加载
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Hashtable LoadForm(Guid Id)
        {
            var _Sys_Menu = db.FindById<Sys_Menu>(Id);
            var _Parent_Menu = db.FindById<Sys_Menu>(_Sys_Menu.Menu_ParentID);
            var _Sys_FunctionList = db.Query<Sys_Function>()
                .Join<Sys_MenuFunction>(w => w.t1.Function_ID == w.t2.MenuFunction_FunctionID & w.t2.MenuFunction_MenuID == Id)
                .OrderBy(w => w.SqlStr($"convert(int,{nameof(w.t1.Function_Num)})"))
                .Select(w => new
                {
                    w.t1,
                    w.t2,
                    SqlStr = $"case when {nameof(w.t2.MenuFunction_ID)} is null then 0 else 1 end [State] "
                })
                .ToList<dynamic>();

            var _Form = AppBase.ObjectToHashtable(new
            {
                status = 1,
                pname = _Parent_Menu.Menu_Name.ToStr(),
                Sys_FunctionList = _Sys_FunctionList,
                Sys_MenuFunctionList = new List<dynamic>()
            }, _Sys_Menu);

            return _Form;
        }

        #endregion

        #region  创建系统左侧菜单

        /// <summary>
        /// 根据角色ID 获取菜单
        /// </summary>
        /// <returns></returns>
        public async Task<List<Sys_Menu>> GetMenuByRoleIDAsync(Account _Account)
        {
            var _Sys_MenuAllList = await db.Query<Sys_Menu>(w => w.t1.Menu_IsShow == 1).OrderBy(w => w.t1.Menu_Num).ToListAsync();
            if (_Account.IsSuperManage) return _Sys_MenuAllList;

            var _Sys_MenuList = await db
                .Query<Sys_RoleMenuFunction>(w => w.t1.RoleMenuFunction_RoleID == _Account.RoleID)
                .Join<Sys_Function>(w => w.t1.RoleMenuFunction_FunctionID == w.t2.Function_ID)
                .Join<Sys_Menu>(w => w.t1.RoleMenuFunction_MenuID == w.t3.Menu_ID)
                .Where(w => w.t2.Function_Num == "10")
                .Select(w => w.t3)
                .ToSql(out DbFrame.BaseClass.SQL Sql)
                .ToListAsync<Sys_Menu>();

            var _New_Sys_MenuList = new List<Sys_Menu>();

            for (int i = 0; i < _Sys_MenuList.Count; i++)
            {
                var item = _Sys_MenuList[i];
                if (!_Sys_MenuList.Any(w => w.Menu_ID == item.Menu_ParentID.ToGuid()) & !_New_Sys_MenuList.Any(w => w.Menu_ID == item.Menu_ParentID.ToGuid()))
                {
                    var _Menu = _Sys_MenuAllList.Find(w => w.Menu_ID == item.Menu_ParentID);
                    if (_Menu != null) _New_Sys_MenuList.Add(_Menu);
                }
                _New_Sys_MenuList.Add(item);
            }

            return _New_Sys_MenuList.OrderBy(w => w.Menu_Num).ToList();
        }

        /// <summary>
        /// 创建菜单
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="_StringBuilder"></param>
        public void CreateMenus(Guid Id, List<Sys_Menu> _Sys_Menu_List, StringBuilder _StringBuilder)
        {
            var _Parent_List = new List<Sys_Menu>();
            if (Id == Guid.Empty)
                _Parent_List = _Sys_Menu_List.Where(w => w.Menu_ParentID == null || w.Menu_ParentID == Guid.Empty).ToList();
            else
                _Parent_List = _Sys_Menu_List.Where(w => w.Menu_ParentID == Id).ToList();

            if (_Parent_List.Count > 0)
            {
                if (Id == Guid.Empty)
                {
                    _StringBuilder.Append("<ul class=\"metismenu\" id=\"adminMenu\">");

                    _StringBuilder.Append("<li hzy-router=\"#!首页#!/Admin/Home/Main/\"><a href=\"javascript:void(0)\"><i class=\"fas fa-tachometer-alt\"></i><span>首页</span></a></li>");
                }
                else
                    _StringBuilder.Append("<ul class=\"mm-collapse\">");

                foreach (var item in _Parent_List)
                {
                    var _Child_List = _Sys_Menu_List.FindAll(w => w.Menu_ParentID != null && w.Menu_ParentID == item.Menu_ID);

                    if (_Child_List.Count > 0)
                    {
                        _StringBuilder.Append("<li>");

                        _StringBuilder.AppendFormat("<a class=\"has-arrow\" href=\"javascript:void(0)\" aria-expanded=\"false\"><i class=\"{0}\"></i><span>{1}</span></a>", item.Menu_Icon, item.Menu_Name);

                        this.CreateMenus(item.Menu_ID, _Sys_Menu_List, _StringBuilder);
                        _StringBuilder.Append("</li>");
                    }
                    else
                    {
                        _StringBuilder.AppendFormat("<li hzy-router=\"#!{0}#!{1}\">", item.Menu_Name, item.Menu_Url);

                        _StringBuilder.AppendFormat("<a href=\"javascript:void(0);var url='{0}';\" aria-expanded=\"false\"><i class=\"{1}\"></i><span>{2}</span></a>", item.Menu_Url, item.Menu_Icon, item.Menu_Name);

                        _StringBuilder.Append("</li>");
                    }
                }

                _StringBuilder.Append("</ul>");

            }

        }

        #endregion  左侧菜单

        /// <summary>
        /// 获取 菜单功能 树
        /// </summary>
        /// <returns></returns>
        public async Task<List<object>> GetMenuZTreeAsync()
        {
            var list = new List<object>();
            var _Sys_Menu_List = await db.Query<Sys_Menu>().OrderBy(w => w.t1.Menu_Num).ToListAsync();
            var _Sys_Function_List = await db.Query<Sys_Function>().OrderBy(w => w.SqlStr($"convert(int,{nameof(w.t1.Function_Num)})")).ToListAsync();
            var _Sys_MenuFunction_List = await db.Query<Sys_MenuFunction>().OrderBy(w => w.t1.MenuFunction_CreateTime).ToListAsync();
            //遍历菜单
            foreach (var item in _Sys_Menu_List)
            {
                list.Add(new
                {
                    id = item.Menu_ID,
                    name = item.Menu_Name + "(" + item.Menu_Num + ")",
                    pId = item.Menu_ParentID,
                    @checked = false,
                    chkDisabled = true
                });
                //判断本次菜单底下是否还有子菜单
                if (!_Sys_Menu_List.Any(w => w.Menu_ParentID == item.Menu_ID))
                {
                    //遍历功能
                    foreach (var _Function in _Sys_Function_List)
                    {
                        //判断是否 该菜单下 是否勾选了 该功能
                        var _Sys_MenuFunction_Any = _Sys_MenuFunction_List.Any(w =>
                         w.MenuFunction_FunctionID == _Function.Function_ID &&
                         w.MenuFunction_MenuID == item.Menu_ID);

                        list.Add(new
                        {
                            id = _Function.Function_ID,
                            name = _Function.Function_Name,
                            pId = item.Menu_ID,
                            @checked = _Sys_MenuFunction_Any,
                            chkDisabled = true
                        });
                    }
                }
            }
            return list;
        }




    }
}
