using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BlogSystem.MVCSite.Filters
{
    /// <summary>
    /// 跨域特性
    /// </summary>
    public class ControllerAllowOriginAttribute : AuthorizeAttribute
    {
        public string[] AllowSites { get; set; }
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (AllowSites != null && AllowSites.Any())
            {
                AllowOriginAttribute.onExcute(filterContext, AllowSites);
            }
        }

    }

    public class AllowOriginAttribute
    {
        public static void onExcute(ControllerContext context, string[] AllowSites)
        {
            var origin = context.HttpContext.Request.Headers["Origin"];
            if (AllowSites.Contains(origin))
            {
                context.HttpContext.Response.AppendHeader("Access-Control-Allow-Origin", origin);//允许特定来源的地址请求
                context.HttpContext.Response.AppendHeader("Access-Control-Allow-Credentials", "true");//允许请求带有cookie等信息
            }
        }
    }
}