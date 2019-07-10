using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HzyAdmin.Areas.Admin.Controllers
{
    using DbFrame.DbContext.SqlServer;
    using Toolkit;
    using Toolkit.Enums;
    using Entitys.SysClass;
    using System.Collections;
    using Microsoft.AspNetCore.Http;
    using System.IO;
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;
    using System.Data;

    [ApiExplorerSettings(IgnoreApi = true)]
    [Aop.AdminActionFilterFilter]
    [Area(nameof(ApiVersionsEnum.Admin))]
    public class AdminBaseController : Controller
    {
        /// <summary>
        /// 数据访问对象
        /// </summary>
        public DbContextSqlServer db => Logic.Class.AppBase.db;

        /// <summary>
        /// 当前用户信息
        /// </summary>
        public Account _Account { get; set; }

        /// <summary>
        /// 是否忽略Session检查
        /// </summary>
        public bool IgnoreSessionCheck { get; set; } = false;

        /// <summary>
        /// 菜单标识
        /// </summary>
        public string MenuKey { get; set; }

        /// <summary>
        /// 表单标识
        /// </summary>
        public string FormKey { get; set; }

        /// <summary>
        /// 打印标题
        /// </summary>
        public string PrintTitle { get; set; } = string.Empty;

        /// <summary>
        /// 将  FormCollection  转换为  Dictionary
        /// </summary>
        /// <returns></returns>
        [NonAction]
        protected Hashtable FormCollectionToHashtable(IFormCollection fc)
        {
            var _Hashtable = new Hashtable();
            if (fc != null)
            {
                fc.Keys.ToList().ForEach(item =>
                {
                    _Hashtable.Add(item, System.Net.WebUtility.UrlDecode(fc[item]));
                });
            }

            var _Headers = Tools.HttpHelper.Request.Headers;
            var url = _Headers.ContainsKey("Referer") ? _Headers["Referer"].ToStr() : "";
            if (!string.IsNullOrEmpty(url))
            {
                var dic = this.GetUrlQueryString(url);
                foreach (var item in dic)
                {
                    if (!_Hashtable.ContainsKey(item.Key)) _Hashtable.Add(item.Key, item.Value);
                }
            }

            return _Hashtable;
        }

        /// <summary>
        /// 根据地址字符串获取参数
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        [NonAction]
        public Dictionary<string, object> GetUrlQueryString(string Url)
        {
            var di = new Dictionary<string, object>();
            if (Url.Contains("?"))
            {
                Url = Url.Substring(Url.IndexOf("?") + 1);
                string[] str;
                if (Url.Contains("&"))
                {
                    str = Url.Split('&');
                    foreach (var item in str)
                    {
                        if (item.Contains("="))
                        {
                            di.Add(item.Split('=')[0], (item.Split('=')[1] == "null") ? null : item.Split('=')[1]);
                        }
                    }
                }
                else
                {
                    if (Url.Contains("="))
                    {
                        str = Url.Split('=');
                        di.Add(str[0], str[1]);
                    }
                }
            }
            return di;
        }

        [NonAction]
        public IActionResult Success()
        {
            return new JsonResult(new { status = 1 });
        }

        [NonAction]
        public IActionResult Success(object Data)
        {
            return new JsonResult(Data);
        }

        /// <summary>
        /// 数据源
        /// </summary>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [NonAction]
        public virtual PagingEntity GetPagingEntity(Hashtable query, int page = 1, int rows = 20) => throw new Exception("未实现!");

        /// <summary>
        /// 列表页接口
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual IActionResult GetDataSource(IFormCollection fc, int page = 1, int rows = 20)
        {
            var query = this.FormCollectionToHashtable(fc);
            var pe = this.GetPagingEntity(query, page, rows);
            return Json(new
            {
                status = 1,
                column = pe.ColModel,
                rows = pe.List,
                page = page,
                total = pe.Counts,
                pageCount = pe.PageCount
            });
        }

        /// <summary>
        /// 处理上传文件
        /// </summary>
        /// <param name="_IFormFile"></param>
        /// <param name="_WebRootPath"></param>
        /// <param name="Format">文件格式</param>
        /// <param name="Check">执行前 验证回调</param>
        /// <returns></returns>
        public string HandleUpFile(IFormFile _IFormFile, string _WebRootPath, string[] Format, Action<IFormFile> Check = null)
        {
            Check?.Invoke(_IFormFile);

            string ExtensionName = Path.GetExtension(_IFormFile.FileName).ToLower().Trim();//获取后缀名

            if (Format != null && !Format.Contains(ExtensionName.ToLower()))
            {
                throw new MessageBox("请上传后缀名为：" + string.Join("、", Format) + " 格式的文件");
            }

            if (!Directory.Exists(_WebRootPath + "\\content\\upFile\\"))
                Directory.CreateDirectory(_WebRootPath + "\\content\\upFile\\");
            string filePath = "/content/upFile/" + Guid.NewGuid() + ExtensionName;
            // 创建新文件
            using (FileStream fs = System.IO.File.Create(_WebRootPath + filePath))
            {
                _IFormFile.CopyTo(fs);
                // 清空缓冲区数据
                fs.Flush();
            }

            return filePath;
        }

        /// <summary>
        /// 导出EXCEL
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual IActionResult ExportExcel(IFormCollection fc, int page = 1, int rows = 1000000)
        {
            var query = this.FormCollectionToHashtable(fc);

            foreach (var item in Request.Query.Keys)
            {
                if (!fc.ContainsKey(item.ToString()))
                    query.Add(item.ToString(), Request.Query[item].ToStr());
            }

            var _Headers = Tools.HttpHelper.Request.Headers;
            var url = _Headers.ContainsKey("Referer") ? _Headers["Referer"].ToStr() : "";
            if (!string.IsNullOrEmpty(url))
            {
                var dic = this.GetUrlQueryString(url);
                foreach (var item in dic)
                {
                    if (!query.ContainsKey(item.Key)) query.Add(item.Key, item.Value);
                }
            }

            var pe = GetPagingEntity(query, page, rows);
            return File(DBToExcel(pe), Tools.GetFileContentType[".xls"].ToStr(), Guid.NewGuid().ToString() + ".xls");
        }

        /// <summary>
        /// 表数据转换为EXCEL
        /// </summary>
        /// <param name="pe"></param>
        /// <returns></returns>
        [NonAction]
        public virtual byte[] DBToExcel(PagingEntity pe)
        {
            var dt = pe.Table;
            var list = pe.ColModel;
            HSSFWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = workbook.CreateSheet();

            //填充表头
            IRow dataRow = sheet.CreateRow(0);
            foreach (DataColumn column in dt.Columns)
            {
                if (column.ColumnName.Equals("_ukid"))
                    continue;
                foreach (var item in list)
                {
                    if (column.ColumnName.Equals(item["field"].ToStr()))
                    {
                        dataRow.CreateCell(column.Ordinal).SetCellValue(item["title"].ToStr());
                    }
                }
            }

            //填充内容
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dataRow = sheet.CreateRow(i + 1);
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    if (dt.Columns[j].ColumnName.Equals("_ukid"))
                        continue;
                    dataRow.CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
                }
            }

            //保存
            using (MemoryStream ms = new MemoryStream())
            {
                workbook.Write(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 打印
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public virtual IActionResult Print(IFormCollection fc)
        {
            TempData["Title"] = PrintTitle;
            var query = this.FormCollectionToHashtable(fc);

            foreach (var item in Request.Query.Keys)
            {
                if (!fc.ContainsKey(item.ToString()))
                    query.Add(item.ToString(), Request.Query[item].ToStr());
            }

            var pe = GetPagingEntity(query, 1, 10000000);
            return View(AppConfig.PrintPageUrl, pe);
        }







    }
}