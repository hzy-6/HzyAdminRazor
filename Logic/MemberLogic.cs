using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    using System.Collections;
    using Toolkit;
    using Logic.Class;
    using Entitys;
    using Entitys.SysClass;
    using DbFrame;
    using NPOI.SS.UserModel;

    public class MemberLogic : BaseLogic
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
                .Query<Member>()
                .Join<Sys_User>(w => w.t1.Member_UserID == w.t2.User_ID)
                .WhereIF(!string.IsNullOrEmpty(Query["Member_Name"].ToStr()), w => w.t1.Member_Name.Contains(Query["Member_Name"].ToStr()))
                .WhereIF(!string.IsNullOrEmpty(Query["User_Name"].ToStr()), w => w.t2.User_Name.Contains(Query["User_Name"].ToStr()));

            if (string.IsNullOrEmpty(Query["sortName"].ToStr()))
                _Query.OrderBy(w => new { desc = w.t1.Member_Num });//默认排序字段
            else
                _Query.OrderBy(w => Query["sortName"].ToStr() + " " + Query["sortOrder"].ToStr());//前端自动排序

            var IQuery = _Query.Select(w => new
            {
                w.t1.Member_Photo,
                w.t1.Member_Num,
                w.t1.Member_Name,
                w.t1.Member_Phone,
                w.t1.Member_Sex,
                w.t2.User_Name,
                w.t1.Member_CreateTime,
                _ukid = w.t1.Member_ID
            });

            IQuery.TakePage(Page, Rows, out int TotalCount);

            return this.GetPagingEntity(IQuery, TotalCount, Rows);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string Save(Member model)
        {
            db.Commit(() =>
            {
                if (model.Member_ID.ToGuid() == Guid.Empty)
                {
                    model.Member_ID = db.Insert(model).ToGuid();
                    if (model.Member_ID.ToGuid() == Guid.Empty) throw new MessageBox(this.ErrorMessage);
                }
                else
                {
                    if (!db.UpdateById(model)) throw new MessageBox(this.ErrorMessage);
                }
            });

            return model.Member_ID.ToGuidStr();
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
                    db.DeleteById<Member>(item);
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
            var _MemberM = db.FindById<Member>(Id);
            var _Sys_UserM = db.FindById<Sys_User>(_MemberM.Member_UserID);

            var _Form = ObjectToHashtable(new { status = 1 }, _MemberM, _Sys_UserM);

            if (_Form.ContainsKey("User_Pwd")) _Form.Remove("User_Pwd");
            //格式化 日期
            _Form["Member_Birthday"] = _Form["Member_Birthday"].ToDateTimeFormat();

            return _Form;
        }

        #endregion

        /// <summary>
        /// 导入excel 数据到数据库中
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="_Action"></param>
        public void ExcelToDb(System.IO.Stream fileStream, Action<string> _Action)
        {
            StringBuilder errorMsg = new StringBuilder();
            //excel工作表
            ISheet sheet = null;
            //根据文件流创建excel数据结构,NPOI的工厂类WorkbookFactory会自动识别excel版本，创建出不同的excel数据结构
            IWorkbook workbook = WorkbookFactory.Create(fileStream);
            sheet = workbook.GetSheetAt(0);
            IRow row = null;
            db.Commit(() =>
            {
                if (sheet.LastRowNum > 0)
                {
                    for (int i = 0; i <= sheet.LastRowNum; i++)
                    {
                        row = sheet.GetRow(i);
                        if (row == null) continue;
                        int rowNum = i + 1;
                        if (i > 0)//忽略表头
                        {
                            //var hymc = row.GetCell(0) == null ? "" : NPOIHelper.GetCellValue(row.GetCell(0)).ToStr();//用户名称
                            //var hydh = row.GetCell(1) == null ? "" : NPOIHelper.GetCellValue(row.GetCell(1)).ToStr();//用户电话
                            //var xb = row.GetCell(2) == null ? "" : NPOIHelper.GetCellValue(row.GetCell(2)).ToStr();//性别

                            ///**********开始你的逻辑部分 start***********/

                            //if (string.IsNullOrEmpty(hymc))
                            //{
                            //    errorMsg.Append(string.Format("第{0}行的会员名称不能为空", rowNum)); break;
                            //}

                            ////得到信息 写入数据库
                            //db.Insert<Member>(new Member
                            //{
                            //    Member_Name = hymc,
                            //    Member_Phone = hydh.ToInt32(),
                            //    Member_Sex = xb
                            //}, li);

                            //throw new MessageBox("这里只是做一个 例子！");

                            /**********开始你的逻辑部分 end***********/

                        }
                    }
                }
                else
                {
                    errorMsg.Append("未找到任何数据");
                }

            });

            _Action(errorMsg.ToString());

        }

    }
}
