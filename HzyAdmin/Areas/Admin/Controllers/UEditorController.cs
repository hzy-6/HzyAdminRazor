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
        }

    }
}