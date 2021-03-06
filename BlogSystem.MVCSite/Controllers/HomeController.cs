﻿using BlogSystem.BLL;
using BlogSystem.DTO;
using BlogSystem.IBLL;
using BlogSystem.Models;
using BlogSystem.MVCSite.Filters;
using BlogSystem.MVCSite.Models.UserViewModels;
using BlogSystem.MVCSite.Tools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BlogSystem.MVCSite.Controllers
{
    [ControllerAllowOrigin(AllowSites = new string[] { "http://vue.xiaowoc.cn" })]
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            IArticleManager articleManager = new ArticleManager();
            IUserManager userManager = new UserManager();
            ViewBag.PopularUser = await userManager.GetFamousUser(10);
            ViewBag.ArticlesCount = await articleManager.GetArticleDataCount();//查找文章总数
            ViewBag.UsersCount = await userManager.GetUserDataCount();//查找用户总数
            return View(await articleManager.GetFamousArticle(5));
        }

        [HttpGet]
        public async Task<ActionResult> GetIndex()
        {
            IArticleManager articleManager = new ArticleManager();
            IUserManager userManager = new UserManager();
            var popularUser = await userManager.GetFamousUser(10);
            var articlesCount = await articleManager.GetArticleDataCount();//查找文章总数
            var usersCount = await userManager.GetUserDataCount();//查找用户总数
            var famousArticle = await articleManager.GetFamousArticle(5);//查找喜欢数量最多的
            return Json(new { status = "ok", popularUser, articlesCount, usersCount, famousArticle }, JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //public ActionResult Register()
        //{
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(RegisterViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        IUserManager userManager = new UserManager();
        //        var passWord = Md5Helper.Md5(model.Password);
        //        if (await userManager.Register(model.Email, passWord))
        //        {
        //            ////注册成功后跳转登陆
        //            //Response.Write("<script>alert('注册成功');location.href='/Home/Login';</script>");
        //            //注册成功后自动登陆
        //            var user = await userManager.GetUserByEmail(model.Email);
        //            Session["loginName"] = user.Email;//将邮箱地址存进session
        //            Session["userId"] = user.Id;//将用户id存进session
        //            Response.Write("<script>alert('注册成功');location.href='/Home/Login';</script>");
        //        }
        //    }
        //    ModelState.AddModelError(string.Empty, "注册失败");
        //    return View(model);
        //}

        [HttpPost]
        public async Task<ActionResult> Register(string email, string password, string confirmPassword)
        {
            //不可为空，账号必须是邮箱格式，邮箱不可重复，密码验证正确
            if (email == null || password == null || confirmPassword == null || email.Trim() == "" || password.Trim() == "" || confirmPassword.Trim() == "")
            {
                return Json(new { status = "fail", result = "提交的数据不完整，请重试！" }, JsonRequestBehavior.AllowGet);
            }
            if (password != confirmPassword)
            {
                return Json(new { status = "fail", result = "两次输入的密码不一致！" }, JsonRequestBehavior.AllowGet);
            }
            Regex RegEmail = new Regex("^[\\w-]+@[\\w-]+\\.(com|net|org|edu|mil|tv|biz|info)$");//w 英文字母或数字的字符串，和 [a-zA-Z0-9] 语法一样 
            Match m = RegEmail.Match(email);
            if (!m.Success)
            {
                return Json(new { status = "fail", result = "账号必须是邮箱格式的哦！" }, JsonRequestBehavior.AllowGet);
            }
            IUserManager userManager = new UserManager();
            UserInformationDto user = await userManager.GetUserByEmail(email);
            if (user != null)//已经有人使用了该邮箱
            {
                return Json(new { status = "fail", result = "该邮箱已被使用！" }, JsonRequestBehavior.AllowGet);
            }
            var passWord = Md5Helper.Md5(confirmPassword);
            if (!await userManager.Register(email, passWord))
            {
                return Json(new { status = "fail", result = "注册失败！" }, JsonRequestBehavior.AllowGet);
            }
            //注册成功后自动登陆
            var registerUser = await userManager.GetUserByEmail(email);
            Session["loginName"] = registerUser.Email;//将邮箱地址存进session
            Session["userId"] = registerUser.Id;//将用户id存进session
            return Json(new { status = "ok", result = "注册成功！" }, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<ActionResult> IsEmailInUse(string email)
        {
            IUserManager userManager = new UserManager();
            var user = await userManager.GetUserByEmail(email);
            if (user != null)
            {
                return Json($"邮箱{email}已经被使用了", JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //public ActionResult Login()
        //{
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (Session["userId"] != null)
        //        {
        //            Session["loginName"] = null;
        //            Session["userId"] = null;
        //        }
        //        if (Request.Cookies["loginName"] != null)
        //        {
        //            Response.Cookies["loginName"].Expires = DateTime.Now.AddDays(-1);
        //            Response.Cookies["userId"].Expires = DateTime.Now.AddDays(-1);
        //        }
        //        IUserManager userManager = new UserManager();
        //        Guid userId;
        //        var passWord = Md5Helper.Md5(model.LoginPwd);
        //        if (userManager.Login(model.Email, passWord, out userId))
        //        {
        //            //跳转
        //            //使用cookie或者是session
        //            string userIdToken = JwtHelper.SetJwtEncode(userId.ToString(), 86400);
        //            if (model.RemenberMe)
        //            {
        //                Response.Cookies.Add(new HttpCookie("loginName")//将邮箱地址存进cookie
        //                {
        //                    Value = model.Email,
        //                    Expires = DateTime.Now.AddDays(1)
        //                });
        //                Response.Cookies.Add(new HttpCookie("userId")//将用户id存进cookie
        //                {
        //                    Value = userIdToken,
        //                    Expires = DateTime.Now.AddDays(1)
        //                });
        //            }
        //            //存完cookie也要补充一份session
        //            Session["loginName"] = model.Email;//将邮箱地址存进session
        //            Session["userId"] = userId;//将用户id存进session
        //            if (returnUrl != null && returnUrl.StartsWith(Request.Url.Scheme + "://" + Request.Url.Host))//验证是否为本地链接
        //            {
        //                return Redirect(returnUrl);
        //            }
        //            else
        //            {
        //                return RedirectToAction(nameof(Index));
        //            }
        //        }
        //        else
        //        {
        //            ModelState.AddModelError(string.Empty, "您的账号密码有误");
        //        }
        //    }
        //    return View(model);
        //}

        [HttpPost]
        public async Task<ActionResult> Login(string email, string password, bool? rememberMe)
        {
            //数据不完整不可登陆
            if (email == null || email.Trim() == "" || password == null || password.Trim() == "" || rememberMe == null)
            {
                return Json(new { status = "fail", result = "提交的数据不完整，请重试" }, JsonRequestBehavior.AllowGet);
            }
            //在登陆状态时再请求登陆则清空上一个账号信息
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
            var passWord = Md5Helper.Md5(password);
            if (!userManager.Login(email, passWord, out userId))//找不到用户信息，登陆失败
            {
                return Json(new { status = "fail", result = "您的账号密码有误" }, JsonRequestBehavior.AllowGet);
            }
            //使用cookie或者是session
            string userIdToken = JwtHelper.SetJwtEncode(userId.ToString(), 86400);//生成1天的jwt信息
            if (rememberMe.Value)
            {
                Response.Cookies.Add(new HttpCookie("loginName")//将邮箱地址存进cookie
                {
                    Value = email,
                    Expires = DateTime.Now.AddDays(1)
                });
                Response.Cookies.Add(new HttpCookie("userId")//将用户id存进cookie
                {
                    Value = userIdToken,
                    Expires = DateTime.Now.AddDays(1)
                });
            }
            //存完cookie也要补充一份session，自动登陆
            Session["loginName"] = email;//将邮箱地址存进session
            Session["userId"] = userId;//将用户id存进session
            return Json(new { status = "ok", result = "登陆成功", userId }, JsonRequestBehavior.AllowGet);
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
            return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> ChangePassword(string oldPwd, string newPwd, string confirmNewPwd)
        {
            //获取当前登陆的id，cookie的id需要解密
            string userCookieId = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userCookieId, out message))
                {
                    return Json(new { status = "fail", result = message, code = 401 }, JsonRequestBehavior.AllowGet);
                }
            }
            string userId = Session["userId"] == null ? userCookieId : Session["userId"].ToString();//优先获取session的id
            if (userId == null || userId.Trim() == "")
            {
                return Json(new { status = "fail", result = "获取用户信息失败，请检查登陆状态" }, JsonRequestBehavior.AllowGet);
            }
            if (oldPwd == null || newPwd == null || confirmNewPwd == null || oldPwd.Trim() == "" || newPwd.Trim() == "" || confirmNewPwd.Trim() == "")
            {
                return Json(new { status = "fail", result = "提交的数据不可为空" }, JsonRequestBehavior.AllowGet);
            }
            if (newPwd != confirmNewPwd)//防止js验证不生效（被禁用），这里在补充验证
            {
                return Json(new { status = "fail", result = "新密码与确认新密码不一致，请重试！" }, JsonRequestBehavior.AllowGet);
            }
            IUserManager userManager = new UserManager();
            oldPwd = Md5Helper.Md5(oldPwd);
            confirmNewPwd = Md5Helper.Md5(confirmNewPwd);
            if (!await userManager.ChangePassword(Guid.Parse(userId), oldPwd, confirmNewPwd))
            {
                return Json(new { status = "fail", result = "旧密码错误，请重试！" }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { status = "ok", result = "修改成功！" }, JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //[BlogSystemAuth]
        //public ActionResult EditPwd()
        //{
        //    return View(new EditPwdViewModel() { Email = Session["loginName"].ToString() });
        //}

        //[HttpPost]
        //[BlogSystemAuth]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> EditPwd(EditPwdViewModel model)
        //{
        //    string email = Session["loginName"].ToString();
        //    Guid userId = Guid.Parse(Session["userId"].ToString());
        //    if (ModelState.IsValid)
        //    {
        //        IUserManager userManager = new UserManager();
        //        if (await userManager.ChangePassword(userId, model.OldPassword, model.ConfirmNewPassword))
        //        {
        //            Response.Write("<script>alert('密码修改成功');location.href='/Home/Index';</script>");
        //            //return RedirectToAction(nameof(Index));
        //        }
        //    }
        //    ModelState.AddModelError(string.Empty, "修改失败");
        //    model.Email = email;
        //    return View(model);
        //}

        //[HttpGet]
        //[BlogSystemAuth]
        //public async Task<ActionResult> EditInfo()
        //{
        //    IUserManager userManager = new UserManager();
        //    Guid userId = Guid.Parse(Session["userId"].ToString());
        //    var data = await userManager.GetUserById(userId);
        //    EditInfoViewModel model = new EditInfoViewModel()
        //    {
        //        Id = data.Id,
        //        Email = data.Email,
        //        FansCount = data.FansCount,
        //        FocusCount = data.FocusCount,
        //        Nickname = data.Nickname,
        //        ImagePath = data.ImagePath
        //    };
        //    return View(model);
        //}

        //[HttpPost]
        //[BlogSystemAuth]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> EditInfo(EditInfoViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (model.ImagePath != null && model.ImagePath != "default.png")//存在图片路径则删除就图片
        //        {
        //            string savepath = Server.MapPath("../Image");
        //            string oldFileName = Path.Combine(savepath, model.ImagePath);
        //            System.IO.File.Delete(oldFileName);
        //        }
        //        string newFileName = ProcessUploadedFile(model.Image);
        //        IUserManager userManager = new UserManager();
        //        Guid userId = Guid.Parse(Session["userId"].ToString());
        //        if (await userManager.ChangeUserInformation(userId, model.Nickname, newFileName) && model.Email == Session["loginName"].ToString())//防止邮箱被修改
        //        {
        //            Response.Write("<script>alert('资料修改成功');location.href='/Home/Index';</script>");
        //        }
        //    }
        //    ModelState.AddModelError(string.Empty, "修改失败");
        //    return View(model);
        //}

        [HttpPost]
        public async Task<ActionResult> ChangeNickName(string nickName)
        {
            if (nickName == null || nickName.Trim() == "")
            {
                return Json(new { status = "fail", result = "昵称不可为空！" }, JsonRequestBehavior.AllowGet);
            }
            IUserManager userManager = new UserManager();
            //获取当前登陆的id，cookie的id需要解密
            string userCookieId = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userCookieId, out message))
                {
                    return Json(new { status = "fail", result = message }, JsonRequestBehavior.AllowGet);
                }
            }
            string userId = Session["userId"] == null ? userCookieId : Session["userId"].ToString();//优先获取session的id
            if (userId == null || userId.Trim() == "")
            {
                return Json(new { status = "fail", result = "获取用户信息失败，请检查登陆状态" }, JsonRequestBehavior.AllowGet);
            }
            if (!await userManager.ChangeUserNickName(Guid.Parse(userId), nickName))
            {
                return Json(new { status = "fail", result = "修改失败" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> ChangeImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                return Json(new { status = "fail", result = "图片不可为空，请重试！" }, JsonRequestBehavior.AllowGet);
            }
            IUserManager userManager = new UserManager();
            //获取当前登陆的id，cookie的id需要解密
            string userCookieId = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userCookieId, out message))
                {
                    return Json(new { status = "fail", result = message }, JsonRequestBehavior.AllowGet);
                }
            }
            string userId = Session["userId"] == null ? userCookieId : Session["userId"].ToString();//优先获取session的id
            if (userId == null || userId.Trim() == "")
            {
                return Json(new { status = "fail", result = "获取用户信息失败，请检查登陆状态" }, JsonRequestBehavior.AllowGet);
            }
            UserInformationDto user = await userManager.GetUserById(Guid.Parse(userId));
            if (user.ImagePath != null && user.ImagePath != "default.png")//存在图片路径则删除就图片
            {
                string savepath = Server.MapPath("../Image");
                string oldFileName = Path.Combine(savepath, user.ImagePath);
                System.IO.File.Delete(oldFileName);
            }
            string newFileName = ProcessUploadedFile(file);
            if (!await userManager.ChangeUserImage(Guid.Parse(userId), newFileName))
            {
                return Json(new { status = "fail", result = "修改失败" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = "ok", path = newFileName }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> UserDetails(Guid? id)
        {
            //获取当前登陆的id，cookie的id需要解密
            string userCookieId = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userCookieId, out message))
                {
                    ErrorController.message = message;
                    return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
                }
            }
            string userId = Session["userId"] == null ? userCookieId : Session["userId"].ToString();
            if (id == null && userId != null && userId.Trim() != "")
            {
                return RedirectToAction(nameof(UserDetails), new { id = userId });
            }
            IUserManager userManager = new UserManager();
            if (id == null || !await userManager.ExistsUser(id.Value))
            {
                ErrorController.message = "未能找到对应ID的用户，请稍后再试";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
            UserInformationDto user = await userManager.GetUserById(id.Value);
            IArticleManager articleManager = new ArticleManager();
            ViewBag.LatestArticles = await articleManager.GetCurrentUserLatestArticle(5, id.Value, false);//选取5篇最新发布的文章，不含置顶
            ViewBag.TopArticles = await articleManager.GetCurrentUserLatestArticle(100, id.Value, true);//选取100篇最新发布的置顶文章(不足100取找到的最大值)
            ViewBag.ArticlesCount = await articleManager.GetArticleDataCount(user.Id);//查找文章总数
            ViewBag.CategoriesCount = await articleManager.GetCategoryDataCount(user.Id);//查找分类总数
            ViewBag.IsFocused = userId == "" ? false : await userManager.IsFocused(Guid.Parse(userId), id.Value);//id为空也视为没关注
            ViewBag.IsCurrentUser = userId == "" ? false : Guid.Parse(userId) == id.Value ? true : false;//是否为当前登陆用户
            ViewBag.TenTags = await articleManager.GetCategoriesByCount(id.Value, 10);//返回10个分类
            return View(user);
        }

        [HttpGet]
        public async Task<ActionResult> GetUserDetails(Guid? id)
        {
            //获取当前登陆的id，cookie的id需要解密
            string userCookieId = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userCookieId, out message))
                {
                    return Json(new { result = message, status = "fail", code = 401 }, JsonRequestBehavior.AllowGet);//返回错误信息
                }
            }
            string userId = Session["userId"] == null ? userCookieId : Session["userId"].ToString();
            if (id == null && userId != null && userId.Trim() != "")
            {
                return RedirectToAction(nameof(GetUserDetails), new { id = userId });
            }
            IUserManager userManager = new UserManager();
            if (id == null)//未登录不可为空
            {
                return Json(new { code = 401 }, JsonRequestBehavior.AllowGet);//仅返回错误代码跳转登陆，不弹提示
            }
            if (!await userManager.ExistsUser(id.Value))
            {
                return Json(new { result = "未能找到对应ID的用户，请稍后再试", status = "fail" }, JsonRequestBehavior.AllowGet);//返回错误信息
            }
            UserInformationDto userInfo = await userManager.GetUserById(id.Value);
            IArticleManager articleManager = new ArticleManager();
            var latestArticles = await articleManager.GetCurrentUserLatestArticle(5, id.Value, false);//选取5篇最新发布的文章，不含置顶
            var topArticles = await articleManager.GetCurrentUserLatestArticle(100, id.Value, true);//选取100篇最新发布的置顶文章(不足100取找到的最大值)
            var articlesCount = await articleManager.GetArticleDataCount(userInfo.Id);//查找文章总数
            var categoriesCount = await articleManager.GetCategoryDataCount(userInfo.Id);//查找分类总数
            var isFocused = userId == "" ? false : await userManager.IsFocused(Guid.Parse(userId), id.Value);//id为空也视为没关注
            var isCurrentUser = userId == "" ? false : Guid.Parse(userId) == id.Value ? true : false;//是否为当前登陆用户
            var tenTags = await articleManager.GetCategoriesByCount(id.Value, 10);//返回10个分类
            return Json(new { status = "ok", userInfo, latestArticles, topArticles, articlesCount, categoriesCount, isFocused, isCurrentUser, tenTags }, JsonRequestBehavior.AllowGet);
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
        public async Task<ActionResult> Search(string searchWord, int searchType = 0, int pageIndex = 1, int pageSize = 10)
        {//searchType:0-查用户名和标题 1-查标题 2-查用户名
            if (searchWord == "")
            {
                ViewBag.PageCount = 0;//总页数
                ViewBag.PageIndex = pageIndex;//当前页数
                ViewBag.SearchWord = searchWord;//当前关键字
                ViewBag.SearchType = searchType;//当前类型
                return View(new List<ArticleDto>());
            }
            IArticleManager articleManager = new ArticleManager();
            List<ArticleDto> data = await articleManager.GetAllSearchArticles(searchWord, searchType, pageIndex - 1, pageSize);
            int dataCount = await articleManager.GetSearchArticleDataCount(searchWord, searchType);//符合搜索的文章总数
            ViewBag.PageCount = dataCount % pageSize == 0 ? dataCount / pageSize : dataCount / pageSize + 1;//总页数
            ViewBag.DataCount = dataCount;//匹配数量
            ViewBag.PageIndex = pageIndex;//当前页数
            ViewBag.SearchWord = searchWord;//当前关键字
            ViewBag.SearchType = searchType;//当前查询类型
            return View(data);
        }

        [HttpGet]
        public async Task<ActionResult> GetSearch(string searchWord, int searchType = 0, int pageIndex = 1, int pageSize = 10)
        {//searchType:0-查用户名和标题 1-查标题 2-查用户名
            if (searchWord == "")
            {
                return Json(new { status = "fail", result = "搜索关键字为空" }, JsonRequestBehavior.AllowGet); ;//返回错误页面
            }
            IArticleManager articleManager = new ArticleManager();
            List<ArticleDto> data = await articleManager.GetAllSearchArticles(searchWord, searchType, pageIndex - 1, pageSize);
            int dataCount = await articleManager.GetSearchArticleDataCount(searchWord, searchType);//符合搜索的文章总数
            var pageCount = dataCount % pageSize == 0 ? dataCount / pageSize : dataCount / pageSize + 1;//总页数
            return Json(new { status = "ok", data, dataCount, pageCount, pageIndex, pageSize, searchWord, searchType }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ForgetPassword(ForgetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            IUserManager userManager = new UserManager();
            UserInformationDto user = await userManager.GetUserByEmail(model.Email);
            string token;
            //查找的Email是否存在，在就获取id制作token，否则返回不存在
            if (user == null || !await userManager.ExistsUser(user.Id))
            {
                ModelState.AddModelError(string.Empty, "该邮箱不存在，请重试！");
                return View();
            }
            token = JwtHelper.SetJwtEncode((user.Id).ToString(), 600);//jwt有效期十分钟
            string modelError = await userManager.ForgetPassword(token, user.Id, user.Email);
            if (modelError != null)//失败
            {
                ModelState.AddModelError(string.Empty, modelError);
                return View(model);
            }
            //在执行发送邮件里记录错误信息
            string url = ConfigurationManager.AppSettings["MvcUrl"].ToString() + "/Home/ResetPassword?Token=" + token;
            MailHelper.SendEmailDefault(user.Email, url);
            ViewBag.Message = "发送邮件成功！";
            return View("Tips");
        }

        [HttpPost]
        public async Task<ActionResult> PostForgetPassword(string email)
        {
            //邮箱为空，邮箱不正确
            if (email == null || email.Trim() == "")
            {
                return Json(new { status = "fail", result = "提交的数据不完整！" }, JsonRequestBehavior.AllowGet);
            }
            Regex RegEmail = new Regex("^[\\w-]+@[\\w-]+\\.(com|net|org|edu|mil|tv|biz|info)$");//w 英文字母或数字的字符串，和 [a-zA-Z0-9] 语法一样 
            Match m = RegEmail.Match(email);
            if (!m.Success)
            {
                return Json(new { status = "fail", result = "账号必须是邮箱格式的哦！" }, JsonRequestBehavior.AllowGet);
            }
            IUserManager userManager = new UserManager();
            UserInformationDto user = await userManager.GetUserByEmail(email);
            string token;
            //查找的Email是否存在，在就获取id制作token，否则返回不存在
            if (user == null)
            {
                return Json(new { status = "fail", result = "该邮箱不存在，请重试！" }, JsonRequestBehavior.AllowGet);
            }
            token = JwtHelper.SetJwtEncode((user.Id).ToString(), 600);//jwt有效期十分钟
            string modelError = await userManager.ForgetPassword(token, user.Id, user.Email);
            if (modelError != null)//失败
            {
                //在执行发送邮件里记录错误信息
                return Json(new { status = "fail", result = modelError }, JsonRequestBehavior.AllowGet);
            }
            string url = ConfigurationManager.AppSettings["ApiUrl"].ToString() + "/ResetPassword?Token=" + token;
            if (!MailHelper.SendEmailDefault(user.Email, url))
            {
                return Json(new { status = "fail", result = "系统邮箱配置错误，邮件发送失败！" }, JsonRequestBehavior.AllowGet);
            };
            return Json(new { status = "ok", result = "发送邮件成功！" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ResetPassword(string token, ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            string userId; string message;
            //jwt验证是否有效
            if (!JwtHelper.GetJwtDecode(token, out userId, out message))
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }
            string password = Md5Helper.Md5(model.ConfirmPassword);
            IUserManager userManager = new UserManager();
            //验证token内容是否存在
            string modelError = await userManager.ResetPassword(token, Guid.Parse(userId), password);
            if (modelError != null)//失败
            {
                ModelState.AddModelError(string.Empty, modelError);
                return View(model);
            }
            ViewBag.Message = "重置密码成功！";
            return View("Tips");
        }

        [HttpPost]
        public async Task<ActionResult> PostResetPassword(string token, string password, string confirmPassword)
        {
            if (password == null || password.Trim() == "" || confirmPassword == null || confirmPassword.Trim() == "")
            {
                return Json(new { status = "fail", result = "提交的数据不完整！" }, JsonRequestBehavior.AllowGet);
            }
            if (password != confirmPassword)
            {
                return Json(new { status = "fail", result = "两次输入的密码不一致！" }, JsonRequestBehavior.AllowGet);
            }
            string userId; string message;
            //jwt验证是否有效
            if (!JwtHelper.GetJwtDecode(token, out userId, out message))
            {
                return Json(new { status = "fail", result = message }, JsonRequestBehavior.AllowGet);
            }
            string passwordMd5 = Md5Helper.Md5(confirmPassword);
            IUserManager userManager = new UserManager();
            //验证token内容是否存在
            string modelError = await userManager.ResetPassword(token, Guid.Parse(userId), passwordMd5);
            if (modelError != null)//失败
            {
                return Json(new { status = "fail", result = modelError }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = "ok", result = "重置密码成功！" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult IsLogin()
        {
            string userCookieId = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userCookieId, out message))
                {
                    return Json(new { status = "fail", result = message }, JsonRequestBehavior.AllowGet);
                }
            }
            string userId = Session["userId"] == null ? userCookieId : Session["userId"].ToString();//优先获取session的id
            if (userId == null || userId.Trim() == "")
            {
                return Json(new { status = "fail", result = "获取用户信息失败，请检查登陆状态" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = "ok", userId }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 拼接图片存放路径，复制图片到指定路径
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string ProcessUploadedFile(HttpPostedFileBase file)
        {
            string uniqueFileName = null;
            if (file != null)
            {
                //string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");//wwwroot下的images
                //uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;//特殊唯一文件名
                //string filePath = Path.Combine(uploadsFolder, uniqueFileName);//拼接出文件目录文件名
                //model.Photo.CopyTo(new FileStream(filePath, FileMode.Create));//复制图片到指定目录

                string savepath = Server.MapPath("../Image");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;//特殊唯一文件名
                string filePath = Path.Combine(savepath, uniqueFileName);//拼接出文件目录文件名
                file.SaveAs(filePath);//复制图片到指定目录
            }
            return uniqueFileName;
        }
    }
}