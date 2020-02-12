using BlogSystem.MVCSite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BlogSystem.MVCSite.Filters
{
    public class BlogSystemAuthAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            //base.OnAuthorization(filterContext);
            //当有AllowAnonymous特性时跳过
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)
                || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
            {
                return;
            }
            //当cookie数据不为空，session数据为空时。同步cookie的数据到session中
            if (filterContext.HttpContext.Session["userId"] == null && filterContext.HttpContext.Request.Cookies["userId"] != null)
            {
                filterContext.HttpContext.Session["loginName"] = filterContext.HttpContext.Request.Cookies["loginName"].Value;
                string userId; string message;
                JwtHelper.GetJwtDecode(filterContext.HttpContext.Request.Cookies["userId"].Value, out userId, out message);//验证id是否被篡改
                filterContext.HttpContext.Session["userId"] = userId;
            }


            if (!(filterContext.HttpContext.Session["userId"] != null || filterContext.HttpContext.Request.Cookies["userId"] != null))//找不到session或cookie跳转未登录
            {
                //filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary() 
                //{
                //    { "controller","Home"},{ "action","Login"}
                //});
                //没有登陆的情况下跳转登陆界面会带有returnUrl
                //string localPath = filterContext.HttpContext.Request.Url.LocalPath;//本地路径
                //if (localPath.Contains("ArticleList"))//包含列表的添加id参数
                //{
                //    partPath = filterContext.HttpContext.Server.UrlEncode(filterContext.HttpContext.Request.Url.LocalPath) + "?userId=" + filterContext.HttpContext.Session["userId"];
                //}
                //else
                //{
                //    partPath = filterContext.HttpContext.Server.UrlEncode(filterContext.HttpContext.Request.Url.LocalPath);
                //}
                string partPath = filterContext.HttpContext.Server.UrlEncode(filterContext.HttpContext.Request.Url.ToString());//部分路径
                filterContext.Result = new RedirectResult(string.Concat("/Home/Login?returnUrl=", partPath));
            }
        }
    }
}