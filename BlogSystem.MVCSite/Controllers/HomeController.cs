﻿using BlogSystem.BLL;
using BlogSystem.DTO;
using BlogSystem.IBLL;
using BlogSystem.Models;
using BlogSystem.MVCSite.Filters;
using BlogSystem.MVCSite.Models.UserViewModels;
using BlogSystem.MVCSite.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BlogSystem.MVCSite.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            IArticleManager articleManager = new ArticleManager();
            return View(await articleManager.GetFamousArticle(3));
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                IUserManager userManager = new UserManager();
                var passWord = Md5Helper.Md5(model.Password);
                if (await userManager.Register(model.Email, passWord))
                {
                    ////注册成功后跳转登陆
                    //Response.Write("<script>alert('注册成功');location.href='/Home/Login';</script>");
                    //注册成功后自动登陆
                    var user = await userManager.GetUserByEmail(model.Email);
                    Session["loginName"] = user.Email;//将邮箱地址存进session
                    Session["userId"] = user.Id;//将用户id存进session
                    Response.Write("<script>alert('注册成功');location.href='/Home/Login';</script>");
                }
            }
            ModelState.AddModelError(string.Empty, "注册失败");
            return View(model);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<ActionResult> IsEmailInUse(string email)
        {
            IUserManager userManager = new UserManager();
            var user = await userManager.GetUserByEmail(email);
            if (user == null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json($"邮箱{email}已经被使用了", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (Session["userId"] != null)
                {
                    Session["loginName"] = null;
                    Session["userId"] = null;
                }
                if (Request.Cookies["loginName"] != null)
                {
                    Response.Cookies["loginName"].Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies["userId"].Expires = DateTime.Now.AddDays(-1);
                }
                IUserManager userManager = new UserManager();
                Guid userId;
                var passWord = Md5Helper.Md5(model.LoginPwd);
                if (userManager.Login(model.Email, passWord, out userId))
                {
                    //跳转
                    //使用cookie或者是session
                    string userIdToken = JwtHelper.SetJwtEncode(userId.ToString(), 86400);
                    if (model.RemenberMe)
                    {
                        Response.Cookies.Add(new HttpCookie("loginName")//将邮箱地址存进cookie
                        {
                            Value = model.Email,
                            Expires = DateTime.Now.AddDays(1)
                        });
                        Response.Cookies.Add(new HttpCookie("userId")//将用户id存进cookie
                        {
                            Value = userIdToken,
                            Expires = DateTime.Now.AddDays(1)
                        });
                    }
                    //存完cookie也要补充一份session
                    Session["loginName"] = model.Email;//将邮箱地址存进session
                    Session["userId"] = userId;//将用户id存进session
                    if (returnUrl != null && returnUrl.StartsWith(Request.Url.Scheme + "://" + Request.Url.Host))//验证是否为本地链接
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "您的账号密码有误");
                }
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Logout()
        {
            //清空session
            Session.Clear();//清空session对象里的内容
            Session.Abandon();//清空session对象
            //清空所有cookie
            for (int i = 0; i < Request.Cookies.Count; i++)
            {
                Response.Cookies[Request.Cookies[i].Name].Expires = DateTime.Now.AddDays(-1);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [BlogSystemAuth]
        public ActionResult EditPwd()
        {
            return View(new EditPwdViewModel() { Email = Session["loginName"].ToString() });
        }

        [HttpPost]
        [BlogSystemAuth]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPwd(EditPwdViewModel model)
        {
            string email = Session["loginName"].ToString();
            Guid userId = Guid.Parse(Session["userId"].ToString());
            if (ModelState.IsValid)
            {
                IUserManager userManager = new UserManager();
                if (await userManager.ChangePassword(userId, model.OldPassword, model.ConfirmNewPassword))
                {
                    Response.Write("<script>alert('密码修改成功');location.href='/Home/Index';</script>");
                    //return RedirectToAction(nameof(Index));
                }
            }
            ModelState.AddModelError(string.Empty, "修改失败");
            model.Email = email;
            return View(model);
        }

        [HttpGet]
        [BlogSystemAuth]
        public async Task<ActionResult> EditInfo()
        {
            IUserManager userManager = new UserManager();
            Guid userId = Guid.Parse(Session["userId"].ToString());
            var data = await userManager.GetUserById(userId);
            EditInfoViewModel model = new EditInfoViewModel()
            {
                Id = data.Id,
                Email = data.Email,
                FansCount = data.FansCount,
                FocusCount = data.FocusCount,
                SiteName = data.SiteName,
                ImagePath = data.ImagePath
            };
            return View(model);
        }

        [HttpPost]
        [BlogSystemAuth]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditInfo(EditInfoViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.ImagePath != null && model.ImagePath != "default.png")//存在图片路径则删除就图片
                {
                    string savepath = Server.MapPath("../Image");
                    string oldFileName = Path.Combine(savepath, model.ImagePath);
                    System.IO.File.Delete(oldFileName);
                }
                string newFileName = ProcessUploadedFile(model);
                IUserManager userManager = new UserManager();
                Guid userId = Guid.Parse(Session["userId"].ToString());
                if (await userManager.ChangeUserInformation(userId, model.SiteName, newFileName) && model.Email == Session["loginName"].ToString())//防止邮箱被修改
                {
                    Response.Write("<script>alert('资料修改成功');location.href='/Home/Index';</script>");
                }
            }
            ModelState.AddModelError(string.Empty, "修改失败");
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> UserDetails(Guid? id)
        {
            IUserManager userManager = new UserManager();
            if (id == null || !await userManager.ExistsUser(id.Value))
            {
                ErrorController.message = "未能找到对应ID的用户，请稍后再试";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
            UserInformationDto user = await userManager.GetUserById(id.Value);
            IArticleManager articleManager = new ArticleManager();
            ViewBag.FamousArticles = await articleManager.GetCurrentUserFamousArticle(5, id.Value);//选取5篇文章
            ViewBag.ArticlesCount = await articleManager.GetArticleDataCount(user.Id);//查找文章总数
            string userId = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userId, out message);
            }
            string userid = Session["userId"] == null ? userId : Session["userId"].ToString();
            ViewBag.IsFocused = userid == "" ? false : await userManager.IsFocused(Guid.Parse(userid), id.Value);//id为空也视为没关注
            return View(user);
        }

        [HttpPost]
        public async Task<ActionResult> FocusUser(string focusUserId)
        {
            IUserManager userManager = new UserManager();
            string id = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out id, out message))
                {
                    return Json(new { result = message, status = "fail" });
                }
            }
            string userId = Session["userId"] == null ? id : Session["userId"].ToString();
            if (userId == "")
            {
                return Json(new { result = "尚未登陆无法操作！", status = "fail" });
            }
            else if (userId == null || focusUserId == null || userId == "" || focusUserId == "")//id不为空
            {
                return Json(new { result = "部分id不可为空，请稍后刷新再试！", status = "fail" });
            }
            else if (userId == focusUserId)
            {
                return Json(new { result = "关注功能不可以对自己使用哦！", status = "fail" });
            }
            else if (!await userManager.ExistsUser(Guid.Parse(userId)) || !await userManager.ExistsUser(Guid.Parse(focusUserId)))//id不存在
            {
                return Json(new { result = "部分id不存在，请稍后刷新再试！", status = "fail" });
            }
            else if (await userManager.IsFocused(Guid.Parse(userId), Guid.Parse(focusUserId)))//已关注
            {
                return Json(new { result = "用户已处于关注状态！", status = "fail" });
            }
            else
            {
                await userManager.FocusUser(Guid.Parse(userId), Guid.Parse(focusUserId));
                return Json(new { result = "关注成功！", status = "ok" });
            }
        }

        [HttpPost]
        public async Task<ActionResult> UnFocusUser(string focusUserId)
        {
            IUserManager userManager = new UserManager();
            string id = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out id, out message))
                {
                    return Json(new { result = message, status = "fail" });
                }
            }
            string userId = Session["userId"] == null ? id : Session["userId"].ToString();
            if (userId == "")
            {
                return Json(new { result = "尚未登陆无法操作！", status = "fail" });
            }
            else if (userId == null || focusUserId == null || userId == "" || focusUserId == "")//id不为空
            {
                return Json(new { result = "部分id不可为空，请稍后刷新再试！", status = "fail" });
            }
            else if (userId == focusUserId)
            {
                return Json(new { result = "取消关注功能不可以对自己使用哦！", status = "fail" });
            }
            else if (!await userManager.ExistsUser(Guid.Parse(userId)) || !await userManager.ExistsUser(Guid.Parse(focusUserId)))//id不存在
            {
                return Json(new { result = "部分id不存在，请稍后刷新再试！", status = "fail" });
            }
            else if (!await userManager.IsFocused(Guid.Parse(userId), Guid.Parse(focusUserId)))//未关注
            {
                return Json(new { result = "用户已处于未关注状态！", status = "fail" });
            }
            else
            {
                await userManager.UnFocusUser(Guid.Parse(userId), Guid.Parse(focusUserId));
                return Json(new { result = "取消关注成功！", status = "ok" });
            }
        }

        [HttpGet]
        public async Task<ActionResult> Search(string searchWord, int pageIndex = 1, int pageSize = 10)
        {
            if (searchWord == "")
            {
                ViewBag.PageCount = 0;//总页数
                ViewBag.PageIndex = pageIndex;//当前页数
                ViewBag.SearchWord = searchWord;//当前关键字
                return View(new List<ArticleDto>());
            }
            IArticleManager articleManager = new ArticleManager();
            List<ArticleDto> data = await articleManager.GetAllSearchArticles(searchWord, pageIndex - 1, pageSize);
            int dataCount = await articleManager.GetSearchArticleDataCount(searchWord);//符合搜索的文章总数
            ViewBag.PageCount = dataCount % pageSize == 0 ? dataCount / pageSize : dataCount / pageSize + 1;//总页数
            ViewBag.PageIndex = pageIndex;//当前页数
            ViewBag.SearchWord = searchWord;//当前关键字
            return View(data);
        }

        [HttpGet]
        public ActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ForgetPassword(ForgetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                IUserManager userManager = new UserManager();
                UserInformationDto user = await userManager.GetUserByEmail(model.Email);
                string token;
                //查找的Email是否存在，在就获取id制作token，否则返回不存在
                if (user != null && await userManager.ExistsUser(user.Id))
                {
                    token = JwtHelper.SetJwtEncode((user.Id).ToString(), 600);//jwt有效期十分钟
                    string modelError = await userManager.ForgetPassword(token, user.Id, user.Email);
                    if (modelError == null)//成功
                    {
                        //在执行发送邮件里记录错误信息
                        MailHelper.SendEmailDefault(user.Email, token);
                        ViewBag.Message = "发送邮件成功！";
                        return View("Tips");
                    }
                    else//失败
                    {
                        ModelState.AddModelError(string.Empty, modelError);
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "该邮箱不存在，请重试！");
                    return View();
                }
            }
            else
            {
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ResetPassword(string token, ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                string userId; string message;
                //jwt验证是否有效
                if (JwtHelper.GetJwtDecode(token, out userId, out message))
                {
                    string password = Md5Helper.Md5(model.ConfirmPassword);
                    IUserManager userManager = new UserManager();
                    //验证token内容是否存在
                    string modelError = await userManager.ResetPassword(token, Guid.Parse(userId), password);
                    if (modelError == null)//成功
                    {
                        ViewBag.Message = "重置密码成功！";
                        return View("Tips");
                    }
                    else//失败
                    {
                        ModelState.AddModelError(string.Empty, modelError);
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

        /// <summary>
        /// 拼接图片存放路径，复制图片到指定路径
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string ProcessUploadedFile(EditInfoViewModel model)
        {
            string uniqueFileName = null;
            if (model.Image != null)
            {
                //string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");//wwwroot下的images
                //uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;//特殊唯一文件名
                //string filePath = Path.Combine(uploadsFolder, uniqueFileName);//拼接出文件目录文件名
                //model.Photo.CopyTo(new FileStream(filePath, FileMode.Create));//复制图片到指定目录

                string savepath = Server.MapPath("../Image");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;//特殊唯一文件名
                string filePath = Path.Combine(savepath, uniqueFileName);//拼接出文件目录文件名
                model.Image.SaveAs(filePath);//复制图片到指定目录
            }
            return uniqueFileName;
        }
    }
}