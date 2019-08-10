using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.SysClass
{
    using System.Collections;
    using Toolkit;
    using Logic.Class;
    using Entitys.SysClass;
    using DbFrame;

    public class Sys_AppLogLogic : BaseLogic
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
                .Query<Sys_AppLog>()
                .Join<Sys_User>(w => w.t1.AppLog_UserID == w.t2.User_ID)
                .WhereIF(!string.IsNullOrEmpty(Query["AppLog_Api"].ToStr()), w => w.t1.AppLog_Api.Contains(Query["AppLog_Api"].ToStr()));

            if (string.IsNullOrEmpty(Query["sortName"].ToStr()))
                _Query.OrderBy(w => new { desc = w.t1.AppLog_CreateTime });
            else
                _Query.OrderBy(w => Query["sortName"].ToStr() + " " + Query["sortOrder"].ToStr());//前端自动排序

            var IQuery = _Query.Select(w => new
            {
                w.t1.AppLog_IP,
                w.t1.AppLog_Api,
                操作人 = w.t2.User_Name,
                w.t1.AppLog_CreateTime,
                _ukid = w.t1.AppLog_ID
            });

            IQuery.TakePage(Page, Rows, out int TotalCount);

            return this.GetPagingEntity(IQuery, TotalCount, Rows);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string Save(Sys_AppLog model)
        {
            db.Commit(() =>
            {
                if (model.AppLog_ID.ToGuid() == Guid.Empty)
                {
                    model.AppLog_ID = db.Insert(model).ToGuid();
                    if (model.AppLog_ID.ToGuid() == Guid.Empty) throw new MessageBox(this.ErrorMessage);
                }
                else
                {
                    if (db.UpdateById(model)==0) throw new MessageBox(this.ErrorMessage);
                }
            });

            return model.AppLog_ID.ToGuidStr();
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
                    db.DeleteById<Sys_AppLog>(item);
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
            var _Sys_AppLog = db.FindById<Sys_AppLog>(Id);

            var _Form = AppBase.ObjectToHashtable(new { status = 1 }, _Sys_AppLog);

            return _Form;
        }

        #endregion



    }
}
