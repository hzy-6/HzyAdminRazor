using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HzyAdmin.Controllers.Api
{
    using DbFrame.DbContext.SqlServer;
    using Microsoft.Extensions.Configuration;
    using Toolkit;

    /// <summary>
    /// 授权接口   演示： webapi + swagger
    /// </summary>
    public class AuthorizationController : ApiBaseController
    {
        protected IConfiguration _IConfiguration { get; set; }
        public AuthorizationController(IConfiguration configuration)
        {
            this._IConfiguration = configuration;
        }

        /// <summary>
        /// 检查帐户并获取 token
        /// </summary>
        /// <remarks>
        /// {
        ///        status = 1,
        ///        token = _TokenType + new JwtTokenUtil(this._IConfiguration).GetToken(Guid.NewGuid().ToStr()),
        ///        tokenType = _TokenType,
        ///        jumpurl = AppConfig.HomePageUrl + "#!%u9996%u9875#!/Admin/Home/Main/"
        /// }
        /// </remarks>
        /// <param name="UserName"></param>
        /// <param name="UserPasswaord"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Check(string UserName, string UserPasswaord)
        {
            //var _UserID = await _Logic.Checked(UserName, UserPassword, LoginCode);
            if (UserName != "hzy" && UserPasswaord != "123") throw new MessageBox("授权失败!");
            var _TokenType = "Bearer ";

            return this.Json(new
            {
                status = 1,
                token = _TokenType + new JwtTokenUtil().GetToken(Guid.NewGuid().ToStr()),
                tokenType = _TokenType,
                jumpurl = AppConfig.HomePageUrl + "#!%u9996%u9875#!/Admin/Home/Main/"
            });
        }

        /// <summary>
        /// 检查帐户并获取 token
        /// </summary>
        /// <remarks>
        /// {
        ///        status = 1,
        ///        userName = "hzy"
        /// }
        /// </remarks>
        /// <returns></returns>
        [HttpPost(nameof(GetUser)), Aop.ApiCheckTokenFilter]
        public IActionResult GetUser()
        {
            return this.Json(new
            {
                status = 1,
                userName = "hzy"
            });
        }

        //[ApiExplorerSettings(IgnoreApi = true)] //该标记用于 忽略添加到 swagger 接口文档中






    }
}