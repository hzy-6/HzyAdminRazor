using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HzyAdmin.Models;

namespace HzyAdmin.Controllers
{
    using Microsoft.AspNetCore.Diagnostics;
    using Toolkit;
    using Logic;
    using Newtonsoft.Json;

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Login", new { area = "Admin" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var _IExceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var _Error = _IExceptionHandlerFeature?.Error;
            var IsAjaxRequest = Tools.IsAjaxRequest;
            var _ErrorModel = ErrorModel.Model;
            //判断是否是自定义异常类型
            if (_Error is MessageBox)
            {
                HttpContext.Response.StatusCode = 200;
                if (_ErrorModel.status == (int)EMsgStatus.Custom60) return Json(_ErrorModel.Data);
                return Json(ErrorModel.Model);
            }
            else
            {
                Tools.Log.Write(_Error, HttpContext.Connection.RemoteIpAddress.ToString());//nlog 写入日志到 txt
                _ErrorModel = new ErrorModel(_Error);
                if (IsAjaxRequest) return Json(_ErrorModel);
                //return Content(JsonConvert.SerializeObject(_ErrorModel));
                return View(AppConfig.ErrorPageUrl, _ErrorModel);
            }
        }
    }
}
