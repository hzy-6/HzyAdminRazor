using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aop
{
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// swagger 设置默认参数
    /// </summary>
    public class SwaggerParameterFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<IParameter>();
            var descriptor = (context.ApiDescription.ActionDescriptor as ControllerActionDescriptor);
            if (descriptor != null)
            {
                var actionAttributes = descriptor.MethodInfo.GetCustomAttributes(inherit: true);
                bool isAnonymous = actionAttributes.Any(a => a is Aop.ApiCheckTokenFilter);
                //链接中添加accesstoken值 在头部
                if (isAnonymous)
                {
                    operation.Parameters.Add(new NonBodyParameter()
                    {
                        Name = "Authorization",
                        In = "header",//query header body path formData
                        Type = "string",
                        Required = true //是否必选
                    });
                }
            }

            //foreach (var attr in attrs)
            //{
            //    // 如果 Attribute 是我们自定义的验证过滤器
            //    if (attr.GetType() == typeof(AopCheckTokenFilterAttribute))
            //    {
            //        operation.Parameters.Add(new NonBodyParameter()
            //        {
            //            Name = "Authorization",
            //            In = "header",
            //            Type = "string",
            //            Required = false
            //        });
            //    }
            //}
        }

    }
}
