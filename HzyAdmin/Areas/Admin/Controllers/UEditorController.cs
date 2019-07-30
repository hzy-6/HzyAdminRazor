using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using UEditor.Core;

namespace HzyAdmin.Areas.Admin.Controllers
{
    //[EnableCors("ApiAny")]//跨域
    public class UEditorController : AdminBaseController
    {
        private readonly UEditorService _ueditorService;
        public UEditorController(UEditorService ueditorService)
        {
            this.IgnoreSessionCheck = true;
            this._ueditorService = ueditorService;
        }

        //如果是API，可以按MVC的方式特别指定一下API的URI
        [HttpGet, HttpPost]
        public ContentResult Upload()
        {
            var response = _ueditorService.UploadAndGetResponse(HttpContext);
            return Content(response.Result, response.ContentType);


            #region 将资源上传 oss 或者其他服务器 仅供参考

            //if (HttpContext.Request.Query["action"].ToStr() == "config")
            //{
            //    var response = _ueditorService.UploadAndGetResponse(HttpContext);
            //    return Content(response.Result, response.ContentType);
            //}

            ////OSS上传
            //var _Files = HttpContext.Request.Form.Files;
            //var _Url = string.Empty;
            //IFormFile _IFormFile = null;
            //if (_Files.Count > 0)
            //{
            //    foreach (var item in _Files)
            //    {
            //        _IFormFile = item;
            //        var ExtensionName = Path.GetExtension(item.FileName).ToLower().Trim();//获取后缀名
            //        _Url += $"Editor/{Guid.NewGuid()}{ExtensionName}";
            //        item.OpenReadStream().UploadFile(_Url);
            //    }
            //}

            //if (_IFormFile != null)
            //{
            //    return Json(new
            //    {
            //        original = _IFormFile.FileName,
            //        state = "SUCCESS",
            //        title = _IFormFile.FileName,
            //        url = AppConfig.Configs.OSSDomain + _Url
            //    });
            //}

            //return Json(new
            //{
            //    original = "",
            //    state = "Error",
            //    title = "",
            //    url = ""
            //});

            #endregion



        }

    }
}