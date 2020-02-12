using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BlogSystem.MVCSite.Controllers
{
    public class ErrorController : Controller
    {
        public readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public static string message;
        public static Exception exception;

        public ActionResult NotFoundError()
        {
            if (exception == null)//普通跳转
            {
                var ip = HttpContext.Request.UserHostAddress;
                Logger.Warn(ip + " : 查找的页面不存在");
            }
            else//global跳转
            {
                var ip = HttpContext.Request.UserHostAddress;
                Logger.Warn(exception, ip + " : 查找的页面不存在");
            }
            return View();
        }
        public ActionResult IllegalOperationError()
        {
            ViewBag.Error = message;
            var ip = HttpContext.Request.UserHostAddress;
            Logger.Warn(ip + " : " + message);
            return View();
        }
        public ActionResult InternalError()
        {
            ViewBag.Error = message;
            if (exception == null)//普通跳转
            {
                var ip = HttpContext.Request.UserHostAddress;
                Logger.Error(ip + " : " + message);
            }
            else//global跳转
            {
                var ip = HttpContext.Request.UserHostAddress;
                Logger.Error(exception, ip + " : " + message);
            }
            return View();
        }
    }
}