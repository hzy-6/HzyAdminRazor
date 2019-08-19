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

    public class Sys_UserLogic : BaseLogic
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
                .Query<Sys_User>()
                .WhereIF(!string.IsNullOrEmpty(Query["User_Name"].ToStr()), w => w.t1.User_Name.Contains(Query["User_Name"].ToStr()))
                .WhereIF(!string.IsNullOrEmpty(Query["User_LoginName"].ToStr()), w => w.t1.User_LoginName.Contains(Query["User_LoginName"].ToStr()));

            if (string.IsNullOrEmpty(Query["sortName"].ToStr()))
                _Query.OrderBy(w => new { desc = w.t1.User_CreateTime });
            else
                _Query.OrderBy(w => Query["sortName"].ToStr() + " " + Query["sortOrder"].ToStr());//前端自动排序

            var IQuery = _Query.Select(w => new
            {
                w.t1.User_Name,
                w.t1.User_LoginName,
                w.t1.User_Email,
                w.t1.User_CreateTime,
                _ukid = w.t1.User_ID
            });

            IQuery.TakePage(Page, Rows, out int TotalCount);

            return this.GetPagingEntity(IQuery, TotalCount, Rows);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="_Sys_UserRole"></param>
        /// <returns></returns>
        public string Save(Sys_User model, List<Sys_UserRole> Sys_UserRoleList)
        {
            db.Commit(() =>
            {
                if (model.User_ID.ToGuid() == Guid.Empty)
                {
                    model.User_Pwd = string.IsNullOrWhiteSpace(model.User_Pwd) ? "123" : model.User_Pwd; //Tools.MD5Encrypt("123");
                    model.User_ID = db.Insert(model).ToGuid();
                    if (model.User_ID.ToGuid() == Guid.Empty) throw new MessageBox(this.ErrorMessage);
                }
                else
                {
                    //如果 密码字段为空，则不修改该密码
                    var _Success = db.UpdateObjectById(model)
                                        .IgnoreColsIF(string.IsNullOrWhiteSpace(model.User_Pwd), w => w.User_Pwd)
                                        .Execute();
                    if (_Success == 0) throw new MessageBox(this.ErrorMessage);
                }

                //
                if (Sys_UserRoleList.Count > 0)
                {
                    db.Delete<Sys_UserRole>(w => w.t1.UserRole_UserID == model.User_ID);
                    foreach (var item in Sys_UserRoleList)
                    {
                        item.UserRole_UserID = model.User_ID;
                        item.UserRole_ID = db.Insert(item).ToGuid();
                        if (item.UserRole_ID == Guid.Empty) throw new MessageBox(this.ErrorMessage);
                    }
                }

            });

            return model.User_ID.ToGuidStr();
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
                    var _Sys_User = db.FindById<Sys_User>(item);
                    if (_Sys_User.User_IsDelete == 2) throw new MessageBox("该信息无法删除！");
                    db.Delete<Sys_UserRole>(w => w.t1.UserRole_UserID == item);
                    db.DeleteById<Sys_User>(item);
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
            var _Sys_User = db.FindById<Sys_User>(Id);
            var _Sys_UserRole = db.Query<Sys_UserRole>()
                .Join<Sys_Role>(w => w.t1.UserRole_RoleID == w.t2.Role_ID)
                .Where(w => w.t1.UserRole_UserID == Id)
                .OrderByDesc(w => w.t1.UserRole_CreateTime)
                .Select(w => new
                {
                    w.t1,
                    w.t2.Role_Name
                }).ToList<Dictionary<string, object>>();

            var _Form = ObjectToHashtable(new
            {
                status = 1,
                Sys_UserRoleList = _Sys_UserRole
            }, _Sys_User);

            //重要字段移除 不能传递给页面
            //if (_Form.ContainsKey(nameof(Sys_User.User_Pwd))) _Form.Remove(nameof(Sys_User.User_Pwd));

            return _Form;
        }

        #endregion




    }
}
