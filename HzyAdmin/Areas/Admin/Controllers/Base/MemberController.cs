using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HzyAdmin.Areas.Admin.Controllers.Base
{
    using System.Collections;
    using Entitys;
    using Entitys.SysClass;
    using Logic;
    using Logic.SysClass;
    using Microsoft.AspNetCore.Hosting;
    using Toolkit;

    public class MemberController : AdminBaseController
    {
        private IHostingEnvironment _IHostingEnvironment = null;
        private string _WebRootPath = string.Empty;
        public MemberController(IHostingEnvironment IHostingEnvironment)
        {
            this._IHostingEnvironment = IHostingEnvironment;
            _WebRootPath = this._IHostingEnvironment.WebRootPath;
            this.MenuKey = "A-100";
        }

        MemberLogic _Logic = new MemberLogic();

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

        [HttpPost, Aop.AopCheckEntityFilter]
        public IActionResult Save(Member model)
        {
            this.FormKey = _Logic.Save(model);

            if (this.FormKey.ToGuid() != Guid.Empty)
            {
                //OSS上传
                var _Files = HttpContext.Request.Form.Files;
                if (_Files.Count > 0)
                {
                    var file = _Files.Where(w => w.Name == "Member_Photo_Files").FirstOrDefault();
                    if (file != null)
                    {
                        model.Member_Photo = this.HandleUpFile(file, _WebRootPath, new string[] { ".jpg", ".png" });
                    }

                    var _UrlList = new List<string>();
                    foreach (var item in _Files.Where(w => w.Name.Contains("Member_FilePath_Files_")))
                    {
                        _UrlList.Add(this.HandleUpFile(item, _WebRootPath, null));
                    }
                    if (_UrlList.Count > 0)
                    {
                        model.Member_FilePath = string.Join(',', _UrlList);
                        db.UpdateById(model);
                    }

                }
            }

            return this.Success(new { status = 1, formKey = this.FormKey });
        }

        [HttpPost]
        public IActionResult Delete(string Ids)
        {
            _Logic.Delete(Ids);
            return this.Success();
        }

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