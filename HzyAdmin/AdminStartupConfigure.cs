using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HzyAdmin
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Toolkit;
    using Toolkit.Entitys;
    using Toolkit.Enums;
    using DbFrame.DbContext.SqlServer;
    using UEditor.Core;
    using Swashbuckle.AspNetCore.Swagger;
    using System.IO;
    using Toolkit.LogService;
    using Microsoft.Extensions.Logging;

    public static class AdminStartupConfigure
    {
        private static readonly IEnumerable<string> _VersionList = typeof(ApiVersionsEnum).GetEnumNames().ToList().OrderByDescending(w => w);

        public static void AdminConfigureServices(this IServiceCollection services, IConfiguration Configuration, IHostingEnvironment _IWebHostEnvironment)
        {
            #region 跨域配置
            //配置跨域处理
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("AppAny", builder =>
            //    {
            //        builder.AllowAnyOrigin() //允许任何来源的主机访问
            //        .AllowAnyMethod()
            //        .AllowAnyHeader()
            //        .AllowCredentials();//指定处理cookie
            //    });
            //});
            #endregion

            #region 自定义视图
            //自定义 视图 
            services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(item =>
            {
                item.AreaViewLocationFormats.Clear();
                item.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");

                item.AreaViewLocationFormats.Add("/Areas/{2}/Views/{1}/{0}.cshtml");
                item.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/{0}.cshtml");
                item.AreaViewLocationFormats.Add("/Areas/{2}/Views/Sys/{1}/{0}.cshtml");//系统管理
                item.AreaViewLocationFormats.Add("/Areas/{2}/Views/Base/{1}/{0}.cshtml");//基础信息管理
                item.AreaViewLocationFormats.Add("/Areas/{2}/Views/Operate/{1}/{0}.cshtml");//运营管理
                item.AreaViewLocationFormats.Add("/Areas/{2}/Views/Statistics/{1}/{0}.cshtml");//统计管理
            });
            #endregion

            #region Session
            //session 注册
            services.AddSession(item =>
            {
                item.IdleTimeout = TimeSpan.FromMinutes(60 * 2);
                item.Cookie.HttpOnly = true;
                item.Cookie.IsEssential = true;
                //防止edge浏览器访问session丢失问题
                item.Cookie.SameSite = SameSiteMode.None;
            });
            #endregion

            #region HttpContext
            //httpcontext
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            #endregion

            #region AppConfig
            //配置 AppConfigs
            AppConfig.AdminAppConfig = Configuration.GetSection("AppConfig").Get<AdminAppConfig>();
            #endregion

            #region DbFrame
            //注入链接字符串
            DbContextSqlServer.Register(AppConfig.AdminAppConfig.SqlServerConnStr, (tabs) =>
            {
                //注册Models
                Entitys.Class.EntitySet.Register(tabs);
            });
            //将DbContextSqlServer对象注册Logic层静态对象中
            Logic.Class.AppBase.db = new DbContextSqlServer();
            //将DbContextSqlServer对象注册UI层构造函数中
            //services.AddSingleton(Logic.Class.BaseLogic.db.GetType());
            #endregion

            #region JWT
            //jwt
            services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,//是否验证Issuer
                        ValidateAudience = true,//是否验证Audience
                        ValidateLifetime = true,//是否验证失效时间
                        ValidateIssuerSigningKey = true,//是否验证SecurityKey
                        ValidAudience = AppConfig.AdminAppConfig.JwtKeyName,//Audience
                        ValidIssuer = AppConfig.AdminAppConfig.JwtKeyName,//Issuer，这两项和前面签发jwt的设置一致
                        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(AppConfig.AdminAppConfig.JwtSecurityKey))//拿到SecurityKey
                    };
                });
            #endregion

            #region Ueditor
            //Ueditor  编辑器 服务端 注入  configFileRelativePath: "wwwroot/admin/libs/nUeditor/net/config.json", isCacheConfig: false, basePath: "C:/basepath"
            services.AddUEditorService(
                    configFileRelativePath: _IWebHostEnvironment.WebRootPath + "/admin/libs/neditor/net/config.json",
                    isCacheConfig: false,
                    basePath: _IWebHostEnvironment.WebRootPath + "/admin/libs/neditor/net/"
                );
            #endregion

            #region Swagger
            //注册Swagger生成器，定义一个和多个Swagger 文档
            services.AddSwaggerGen(option =>
            {
                foreach (var item in _VersionList)
                {
                    var _SwaggerInfo = new Info();
                    _SwaggerInfo.Version = item;
                    //_SwaggerInfo.Title = "v_" + item;
                    _SwaggerInfo.Description = "author：hzy";
                    //_SwaggerInfo.TermsOfService = "None";
                    option.SwaggerDoc(item, _SwaggerInfo);
                }
                // 为 Swagger JSON and UI设置xml文档注释路径
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);//获取应用程序所在目录（绝对，不受工作目录影响，建议采用此方法获取路径）
                var xmlPath = Path.Combine(basePath, "App.xml");
                option.IncludeXmlComments(xmlPath);
                option.OperationFilter<Aop.SwaggerParameterFilter>(); // 手动高亮
            });

            #endregion
        }

        public static void AdminConfigure(this IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}

            #region nlog 日志
            //初始化NLog
            LogHelper.Init(app, env, loggerFactory);

            //程序启动
            applicationLifetime.ApplicationStarted.Register(() =>
            {
                Tools.Log.Write("App启动");
            });
            //程序正在结束中
            applicationLifetime.ApplicationStopping.Register(() =>
            {
                Tools.Log.Write("App结束中...");
            });
            //程序已结束
            applicationLifetime.ApplicationStopped.Register(() =>
            {
                Tools.Log.Write("App已结束");
            });
            //applicationLifetime.StopApplication();//停止程序
            #endregion

            app.UseExceptionHandler("/Home/Error");

            #region JWT
            //注意添加这一句，启用jwt验证
            app.UseAuthentication();
            #endregion

            #region Session
            //session 注册
            app.UseSession();
            #endregion

            #region HttpContext
            //将 对象 IHttpContextAccessor 注入 HttpContextHelper 静态对象中
            Toolkit.HttpContextService.HttpContextHelper.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
            #endregion

            #region Swagger
            //启用中间件服务生成Swagger作为JSON终结点
            app.UseSwagger();
            //启用中间件服务对swagger-ui，指定Swagger JSON终结点
            app.UseSwaggerUI(option =>
            {
                foreach (var item in _VersionList) option.SwaggerEndpoint($"/swagger/{item}/swagger.json", item);
                option.RoutePrefix = "swagger";
            });
            #endregion

        }


    }
}
