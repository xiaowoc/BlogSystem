﻿using BlogSystem.BLL;
using BlogSystem.DTO;
using BlogSystem.IBLL;
using BlogSystem.Models;
using BlogSystem.MVCSite.Filters;
using BlogSystem.MVCSite.Models.ArticleViewModels;
using BlogSystem.MVCSite.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BlogSystem.MVCSite.Controllers
{
    [ControllerAllowOrigin(AllowSites = new string[] { "http://vue.xiaowoc.cn" })]
    public class ArticleController : Controller
    {
        //[HttpGet]
        //public ActionResult CreateCategory()
        //{
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult CreateCategory(CreateCategoryViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        Guid userId = Guid.Parse(Session["userId"].ToString());
        //        IArticleManager articleManager = new ArticleManager();
        //        articleManager.CreateCategory(model.CategoryName, userId);
        //        return RedirectToAction("CategoryList", new { userId });
        //    }
        //    ModelState.AddModelError(string.Empty, "输入有误");
        //    return View(model);
        //}

        [HttpPost]
        public async Task<ActionResult> AddCategory(string categoryName)
        {
            //不可为空，不可重复,未登录无法提交
            if (categoryName == null || categoryName.Trim() == "")
            {
                return Json(new { status = "fail", result = "分类名称不可为空！" }, JsonRequestBehavior.AllowGet);
            }
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
            if (userId == "")//未登录提醒
            {
                return Json(new { status = "fail", result = "未登陆无法提交！" }, JsonRequestBehavior.AllowGet);
            }
            IArticleManager articleManager = new ArticleManager();
            List<BlogCategoryDto> categoryList = await articleManager.GetAllCategories(Guid.Parse(userId));//获取所有分类名，循环对比是否有重复
            bool isRepeat = false;
            foreach (var cate in categoryList)
            {
                if (cate.BlogCategoryName == categoryName)
                {
                    isRepeat = true;
                    break;
                }
            }
            if (isRepeat)
            {
                return Json(new { status = "fail", result = "添加的分类名称已存在，请勿重复添加！" }, JsonRequestBehavior.AllowGet);
            }
            await articleManager.CreateCategory(categoryName, Guid.Parse(userId));//添加分类
            return Json(new { status = "ok", result = "添加成功！" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> CategoryList(string userId, int pageIndex = 1, int pageSize = 7)
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
            string currentUserId = Session["userId"] == null ? userCookieId : Session["userId"].ToString();
            if (userId == null && currentUserId != null && currentUserId.Trim() != "")
            {
                return RedirectToAction(nameof(CategoryList), new { userId = currentUserId });
            }
            IUserManager userManager = new UserManager();
            IArticleManager articleManager = new ArticleManager();
            Guid userIdGuid;
            Guid.TryParse(userId, out userIdGuid);
            if (!await userManager.ExistsUser(userIdGuid) || userIdGuid == Guid.Empty)
            {
                ErrorController.message = "未能找到对应ID的用户，请稍后再试";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
            var categorys = await articleManager.GetAllCategoriesByUserId(userIdGuid, (pageIndex - 1), pageSize);//数据库是从0开始的
            var dataCount = await articleManager.GetCategoryDataCount(userIdGuid);//分类总数
            ViewBag.PageCount = dataCount % pageSize == 0 ? dataCount / pageSize : dataCount / pageSize + 1;//总页数
            ViewBag.PageIndex = pageIndex;//当前页数
            ViewBag.IsCurrentUser = currentUserId.Trim() == "" ? false : userIdGuid == Guid.Parse(currentUserId) ? true : false;//是否为当前登陆用户
            ViewBag.IsFocused = currentUserId.Trim() == "" ? false : await userManager.IsFocused(Guid.Parse(currentUserId), userIdGuid);//id为空也视为没关注
            ViewBag.RequestId = userIdGuid;//当前请求id
            ViewBag.User = await userManager.GetUserById(userIdGuid);
            ViewBag.ArticlesCount = await articleManager.GetArticleDataCount(userIdGuid);//查找文章总数
            ViewBag.CategoriesCount = dataCount;//查找分类总数
            return View(categorys);
        }

        [HttpGet]
        public async Task<ActionResult> GetCategoryList(string userId, int pageIndex = 1, int pageSize = 7)
        {
            //获取当前登陆的id，cookie的id需要解密
            string userCookieId = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userCookieId, out message))
                {
                    return Json(new { result = message, status = "fail" }, JsonRequestBehavior.AllowGet);//返回错误信息
                }
            }
            string currentUserId = Session["userId"] == null ? userCookieId : Session["userId"].ToString();
            if (userId == null && currentUserId != null && currentUserId.Trim() != "")
            {
                return RedirectToAction(nameof(GetCategoryList), new { userId = currentUserId });
            }
            IUserManager userManager = new UserManager();
            IArticleManager articleManager = new ArticleManager();
            Guid userIdGuid;
            Guid.TryParse(userId, out userIdGuid);
            if (!await userManager.ExistsUser(userIdGuid) || userIdGuid == Guid.Empty)
            {
                return Json(new { result = "未能找到对应ID的用户，请稍后再试", status = "fail" }, JsonRequestBehavior.AllowGet);//返回错误信息
            }
            var categoriesInfo = await articleManager.GetAllCategoriesByUserId(userIdGuid, (pageIndex - 1), pageSize);//数据库是从0开始的
            var dataCount = await articleManager.GetCategoryDataCount(userIdGuid);//分类总数
            var pageCount = dataCount % pageSize == 0 ? dataCount / pageSize : dataCount / pageSize + 1;//总页数
            var isCurrentUser = currentUserId.Trim() == "" ? false : userIdGuid == Guid.Parse(currentUserId) ? true : false;//是否为当前登陆用户
            var isFocused = currentUserId.Trim() == "" ? false : await userManager.IsFocused(Guid.Parse(currentUserId), userIdGuid);//id为空也视为没关注
            var requestId = userIdGuid;//当前请求id
            var userInfo = await userManager.GetUserById(userIdGuid);
            var articlesCount = await articleManager.GetArticleDataCount(userIdGuid);//查找文章总数
            var categoriesCount = dataCount;//查找分类总数
            return Json(new { status = "ok", categoriesInfo, pageCount, pageIndex, pageSize, isCurrentUser, isFocused, requestId, userInfo, articlesCount, categoriesCount }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CreateArticle()
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
            ViewBag.UserId = userId;
            return View();
        }

        [HttpGet]
        public ActionResult GetCreateArticle()
        {
            //获取当前登陆的id，cookie的id需要解密
            string userCookieId = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userCookieId, out message))
                {
                    return Json(new { result = message, status = "fail" }, JsonRequestBehavior.AllowGet);//返回错误信息
                }
            }
            string userId = Session["userId"] == null ? userCookieId : Session["userId"].ToString();
            return Json(new { status = "ok", userId }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> AddArticle(string title, string content, string introContent, Guid[] categoryIds)
        {
            //未登录、内容不为空、标题不为空、分类id不属于自己
            if (title == null || content == null || title.Trim() == "" || content.Trim() == "")//提交的信息为空
            {
                return Json(new { status = "fail", result = "提交的信息不完整，请重试" }, JsonRequestBehavior.AllowGet);//返回错误信息
            }
            //获取当前登陆的id，cookie的id需要解密
            string userCookieId = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userCookieId, out message))
                {
                    return Json(new { status = "fail", result = message }, JsonRequestBehavior.AllowGet);//返回错误信息
                }
            }
            string userId = Session["userId"] == null ? userCookieId : Session["userId"].ToString();
            if (userId == null || userId.Trim() == "")//用户未登录
            {
                return Json(new { status = "fail", result = "获取不到用户信息，请检查登陆状态" }, JsonRequestBehavior.AllowGet);//返回错误信息
            }
            IArticleManager articleManager = new ArticleManager();
            //如果提交的分类不为空，循环自己所有的分类，对比是否正确
            if (categoryIds != null)
            {
                List<BlogCategoryDto> categoryDtoes = await articleManager.GetAllCategories(Guid.Parse(userId));//获取分类对象集合
                List<Guid> currentUserCategoryIds = new List<Guid>();//将分类对象中的分类id整合进一个集合中
                foreach (BlogCategoryDto category in categoryDtoes)
                {
                    currentUserCategoryIds.Add(category.Id);
                }
                for (int i = 0; i < categoryIds.Length; i++)//循环检查提交的分类id是否和自身的分类id有对应
                {
                    if (!currentUserCategoryIds.Contains(categoryIds[i]))//如果提交的分类id与自身的分类id没有匹配项，提示错误
                    {
                        return Json(new { status = "fail", result = "提交的分类与用户所拥有的分类不匹配，请重试！" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            Guid articleId = await articleManager.CreateArticle(title, content, introContent, categoryIds, Guid.Parse(userId));
            return Json(new { status = "ok", result = "提交成功！", articleId }, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //[ValidateInput(false)]
        //public async Task<ActionResult> CreateArticle(CreateArticleViewModel model)
        //{
        //    var userId = Guid.Parse(Session["userId"].ToString());
        //    if (ModelState.IsValid)
        //    {
        //        await new ArticleManager().CreateArticle(model.Title, model.Content, model.CategoryIds, userId);
        //        return RedirectToAction("ArticleList", new { userId });
        //    }
        //    ViewBag.Categories = await new ArticleManager().GetAllCategories(userId);
        //    ModelState.AddModelError(string.Empty, "添加失败");
        //    return View(model);
        //}

        [HttpGet]
        public async Task<ActionResult> ArticleList(string userId, string categoryId = null, int pageIndex = 1, int pageSize = 7)
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
            string currentUserId = Session["userId"] == null ? userCookieId : Session["userId"].ToString();
            if (userId == null && currentUserId != null && currentUserId.Trim() != "")
            {
                return RedirectToAction(nameof(ArticleList), new { userId = currentUserId });
            }
            //需要返回前端 总页数 当前页数 单个页面数量 
            IArticleManager articleManager = new ArticleManager();
            IUserManager userManager = new UserManager();
            Guid userIdGuid;
            Guid.TryParse(userId, out userIdGuid);
            if (!await userManager.ExistsUser(userIdGuid) || userIdGuid == Guid.Empty)
            {
                ErrorController.message = "未能找到对应ID的用户，请稍后再试";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
            //新增按照用户id和分类id查找文章，默认为不找分类id
            Guid categoryIdGuid;
            Guid.TryParse(categoryId, out categoryIdGuid);
            List<ArticleDto> articles;
            int dataCount;
            if (categoryIdGuid == Guid.Empty)
            {
                articles = await articleManager.GetAllArticlesByUserId(userIdGuid, (pageIndex - 1), pageSize);//数据库是从0开始的
                dataCount = await articleManager.GetArticleDataCount(userIdGuid);//文章总数
            }
            else
            {
                articles = await articleManager.GetAllArticlesByUserIdAndCategoryId(userIdGuid, categoryIdGuid, (pageIndex - 1), pageSize);//数据库是从0开始的
                dataCount = await articleManager.GetArticleDataCount(userIdGuid, categoryIdGuid);//文章总数
            }
            ViewBag.PageCount = dataCount % pageSize == 0 ? dataCount / pageSize : dataCount / pageSize + 1;//总页数
            ViewBag.PageIndex = pageIndex;//当前页数
            ViewBag.PageSize = pageSize;//当前显示数目
            ViewBag.Category = categoryIdGuid == Guid.Empty ? null : await articleManager.GetOneCategoryById(categoryIdGuid);
            ViewBag.IsCurrentUser = currentUserId.Trim() == "" ? false : userIdGuid == Guid.Parse(currentUserId) ? true : false;//是否为当前登陆用户
            ViewBag.RequestId = userIdGuid;//当前请求id
            ViewBag.TenTags = await articleManager.GetCategoriesByCount(userIdGuid, 10);//返回10个分类
            ViewBag.ArticlesCount = await articleManager.GetArticleDataCount(userIdGuid);//查找文章总数
            ViewBag.CategoriesCount = await articleManager.GetCategoryDataCount(userIdGuid);//查找分类总数
            ViewBag.IsFocused = currentUserId.Trim() == "" ? false : await userManager.IsFocused(Guid.Parse(currentUserId), userIdGuid);//id为空也视为没关注
            ViewBag.User = await userManager.GetUserById(userIdGuid);
            return View(articles);
        }

        [HttpGet]
        public async Task<ActionResult> GetArticleList(string userId, string categoryId = null, int pageIndex = 1, int pageSize = 7)
        {
            //获取当前登陆的id，cookie的id需要解密
            string userCookieId = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userCookieId, out message))
                {
                    return Json(new { result = message, status = "fail" }, JsonRequestBehavior.AllowGet);//返回错误信息
                }
            }
            string currentUserId = Session["userId"] == null ? userCookieId : Session["userId"].ToString();
            if (userId == null && currentUserId != null && currentUserId.Trim() != "")
            {
                return RedirectToAction(nameof(GetArticleList), new { userId = currentUserId });
            }
            //需要返回前端 总页数 当前页数 单个页面数量 
            IArticleManager articleManager = new ArticleManager();
            IUserManager userManager = new UserManager();
            Guid userIdGuid;
            Guid.TryParse(userId, out userIdGuid);
            if (!await userManager.ExistsUser(userIdGuid) || userIdGuid == Guid.Empty)
            {
                return Json(new { result = "未能找到对应ID的用户，请稍后再试", status = "fail" }, JsonRequestBehavior.AllowGet);//返回错误信息
            }
            //新增按照用户id和分类id查找文章，默认为不找分类id
            Guid categoryIdGuid;
            Guid.TryParse(categoryId, out categoryIdGuid);
            List<ArticleDto> articlesInfo;
            int dataCount;
            if (categoryIdGuid == Guid.Empty)
            {
                articlesInfo = await articleManager.GetAllArticlesByUserId(userIdGuid, (pageIndex - 1), pageSize);//数据库是从0开始的
                dataCount = await articleManager.GetArticleDataCount(userIdGuid);//文章总数
            }
            else
            {
                articlesInfo = await articleManager.GetAllArticlesByUserIdAndCategoryId(userIdGuid, categoryIdGuid, (pageIndex - 1), pageSize);//数据库是从0开始的
                dataCount = await articleManager.GetArticleDataCount(userIdGuid, categoryIdGuid);//文章总数
            }
            var pageCount = dataCount % pageSize == 0 ? dataCount / pageSize : dataCount / pageSize + 1;//总页数
            var category = categoryIdGuid == Guid.Empty ? null : await articleManager.GetOneCategoryById(categoryIdGuid);
            var isCurrentUser = currentUserId.Trim() == "" ? false : userIdGuid == Guid.Parse(currentUserId) ? true : false;//是否为当前登陆用户
            var requestId = userIdGuid;//当前请求id
            var tenTags = await articleManager.GetCategoriesByCount(userIdGuid, 10);//返回10个分类
            var articlesCount = await articleManager.GetArticleDataCount(userIdGuid);//查找文章总数
            var categoriesCount = await articleManager.GetCategoryDataCount(userIdGuid);//查找分类总数
            var isFocused = currentUserId.Trim() == "" ? false : await userManager.IsFocused(Guid.Parse(currentUserId), userIdGuid);//id为空也视为没关注
            var userInfo = await userManager.GetUserById(userIdGuid);
            var pageMatchArticleCount = dataCount;
            return Json(new { status = "ok", articlesInfo, pageMatchArticleCount, pageCount, pageIndex, pageSize, category, isCurrentUser, requestId, tenTags, articlesCount, categoriesCount, isFocused, userInfo }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> ArticleDetails(Guid? id)
        {
            var articleManager = new ArticleManager();
            if (id == null || !await articleManager.ExistsArticle(id.Value))
            {
                ErrorController.message = "未能找到对应ID的文章，请稍后再试";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
            ArticleDto data = await articleManager.GetOneArticleById(id.Value);
            var userManager = new UserManager();
            UserInformationDto user = await userManager.GetUserById(data.userId);
            ViewBag.User = user;
            ViewBag.ArticlesCount = await articleManager.GetArticleDataCount(user.Id);//查找文章总数
            ViewBag.CategoriesCount = await articleManager.GetCategoryDataCount(user.Id);//查找分类总数
            //查询是否已经关注
            string userid = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userid, out message);
            }
            string userId = Session["userId"] == null ? userid : Session["userId"].ToString();
            ViewBag.IsCurrentUser = userId.Trim() == "" ? false : user.Id == Guid.Parse(userId) ? true : false;//是否为当前登陆用户
            ViewBag.IsFocused = userId == "" ? false : await userManager.IsFocused(Guid.Parse(userId), user.Id);//id为空也视为没关注
            ViewBag.TenTags = await articleManager.GetCategoriesByCount(user.Id, 10);//返回10个分类
            return View(data);
        }

        [HttpGet]
        public async Task<ActionResult> GetArticleDetails(Guid? id)
        {
            var articleManager = new ArticleManager();
            if (id == null || !await articleManager.ExistsArticle(id.Value))
            {
                return Json(new { result = "未能找到对应ID的文章，请稍后再试", status = "fail" }, JsonRequestBehavior.AllowGet);//返回错误信息
            }
            ArticleDto articleData = await articleManager.GetOneArticleById(id.Value);
            var userManager = new UserManager();
            UserInformationDto userInfo = await userManager.GetUserById(articleData.userId);
            //查询是否已经关注
            string userid = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userid, out message);
            }
            string userId = Session["userId"] == null ? userid : Session["userId"].ToString();
            var articlesCount = await articleManager.GetArticleDataCount(userInfo.Id);//查找文章总数
            var categoriesCount = await articleManager.GetCategoryDataCount(userInfo.Id);//查找分类总数
            var isCurrentUser = userId.Trim() == "" ? false : userInfo.Id == Guid.Parse(userId) ? true : false;//是否为当前登陆用户
            var isFocused = userId == "" ? false : await userManager.IsFocused(Guid.Parse(userId), userInfo.Id);//id为空也视为没关注
            var tenTags = await articleManager.GetCategoriesByCount(userInfo.Id, 10);//返回10个分类
            return Json(new { status = "ok", articleData, userInfo, articlesCount, categoriesCount, isCurrentUser, isFocused, tenTags }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public async Task<ActionResult> EditArticle(Guid? id)
        {
            //未登录、文章id错误、id为空不可进入
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
            if (userId == null || userId.Trim() == "")//用户未登录
            {
                ErrorController.message = "获取不到用户信息，请检查登陆状态";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }

            //获取当前文章实体
            IArticleManager articleManager = new ArticleManager();
            if (id == null || !await articleManager.ExistsArticle(id.Value))//文章id找不到则跳转文章不存在错误页面
            {
                ErrorController.message = "未能找到对应ID的文章，请稍后再试";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
            var data = await articleManager.GetOneArticleById(id.Value);//要经过上面的判断否则会出错

            if (data.userId != Guid.Parse(userId))//文章作者才可编辑文章
            {
                ErrorController.message = "非本人文章不可进行编辑";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
            //将实体内容放进viewmodel中输出到前端
            ViewBag.UserId = userId;
            ViewBag.Article = data;
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> GetEditArticle(Guid? id)
        {
            //未登录、文章id错误、id为空不可进入
            //获取当前登陆的id，cookie的id需要解密
            string userCookieId = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userCookieId, out message))
                {
                    return Json(new { result = "未能找到对应ID的文章，请稍后再试", status = "fail" }, JsonRequestBehavior.AllowGet);//返回错误信息
                }
            }
            string userId = Session["userId"] == null ? userCookieId : Session["userId"].ToString();
            if (userId == null || userId.Trim() == "")//用户未登录
            {
                return Json(new { result = "获取不到用户信息，请检查登陆状态", status = "fail" }, JsonRequestBehavior.AllowGet);//返回错误信息
            }

            //获取当前文章实体
            IArticleManager articleManager = new ArticleManager();
            if (id == null || !await articleManager.ExistsArticle(id.Value))//文章id找不到则跳转文章不存在错误页面
            {
                return Json(new { result = "未能找到对应ID的文章，请稍后再试", status = "fail" }, JsonRequestBehavior.AllowGet);//返回错误信息
            }
            var article = await articleManager.GetOneArticleById(id.Value);//要经过上面的判断否则会出错

            if (article.userId != Guid.Parse(userId))//文章作者才可编辑文章
            {
                ErrorController.message = "非本人文章不可进行编辑";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
            return Json(new { status = "ok", userId, article }, JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //public async Task<ActionResult> EditCategory(Guid? id)
        //{
        //    //获取当前分类实体
        //    IArticleManager articleManager = new ArticleManager();
        //    if (id == null || !await articleManager.ExistsCategory(id.Value))//分类id找不到则跳转分类不存在错误页面
        //    {
        //        ErrorController.message = "未能找到对应ID的分类，请稍后再试";
        //        return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
        //    }
        //    var data = await articleManager.GetOneCategoryById(id.Value);//要经过上面的判断否则会出错
        //    Guid userId = Guid.Parse(Session["userId"].ToString());
        //    if (data.UserId == userId)//文章作者才可编辑分类
        //    {
        //        //获取所有分类
        //        ViewBag.Categories = await new ArticleManager().GetAllCategories(userId);
        //        //将实体内容放进viewmodel中输出到前端
        //        return View(new EditCategoryViewModel()
        //        {
        //            Id = data.Id,
        //            CategoryName = data.CategoryName
        //        });
        //    }
        //    else
        //    {
        //        if (data.UserId == Guid.Parse("00000000-0000-0000-0000-000000000001"))
        //        {
        //            ErrorController.message = "系统内置分类不可进行编辑";
        //        }
        //        else
        //        {
        //            ErrorController.message = "非本人分类不可进行编辑";
        //        }
        //        return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
        //    }
        //}

        //[HttpPost]
        //[ValidateInput(false)]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> EditArticle(EditArticleViewModel model)
        //{
        //    IArticleManager articleManager = new ArticleManager();
        //    if (!await articleManager.ExistsArticle(model.Id))//文章id找不到则跳转文章不存在错误页面
        //    {
        //        ErrorController.message = "未能找到对应ID的文章，请稍后再试";
        //        return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
        //    }
        //    var data = await articleManager.GetOneArticleById(model.Id);//要经过上面的判断否则会出错
        //    Guid userId = Guid.Parse(Session["userId"].ToString());
        //    if (data.userId == userId)//文章作者才可编辑文章
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            await articleManager.EditArticle(model.Id, model.Title, model.Content, model.CategoryIds);
        //            return RedirectToAction("ArticleList", new { userId = userId });
        //        }
        //        else
        //        {
        //            //获取所有分类
        //            ViewBag.Categories = await new ArticleManager().GetAllCategories(userId);
        //            return View(model);
        //        }
        //    }
        //    else
        //    {
        //        ErrorController.message = "非本人文章不可进行编辑";
        //        return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> EditCategory(EditCategoryViewModel model)
        //{
        //    IArticleManager articleManager = new ArticleManager();
        //    if (!await articleManager.ExistsCategory(model.Id))//分类id找不到则跳转分类不存在错误页面
        //    {
        //        ErrorController.message = "未能找到对应ID的分类，请稍后再试";
        //        return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
        //    }
        //    var data = await articleManager.GetOneCategoryById(model.Id);//要经过上面的判断否则会出错
        //    Guid userId = Guid.Parse(Session["userId"].ToString());
        //    if (data.UserId == userId)//分类作者才可编辑分类
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            List<BlogCategoryDto> categories = await articleManager.GetAllCategories(userId);
        //            foreach (BlogCategoryDto category in categories)
        //            {
        //                if (category.BlogCategoryName == model.CategoryName)//修改后的名字和现有的重复，则提示失败
        //                {
        //                    ModelState.AddModelError(string.Empty, "该名字已存在，请修改后重试！");
        //                    return View(model);
        //                }
        //            }
        //            await articleManager.EditCategory(model.Id, model.CategoryName);
        //            return RedirectToAction("CategoryList", new { userId = userId });
        //        }
        //        else
        //        {
        //            return View(model);
        //        }
        //    }
        //    else
        //    {
        //        if (data.UserId == Guid.Parse("00000000-0000-0000-0000-000000000001"))
        //        {
        //            ErrorController.message = "系统内置分类不可进行编辑";
        //        }
        //        else
        //        {
        //            ErrorController.message = "非本人分类不可进行编辑";
        //        }
        //        return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
        //    }
        //}

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> EditArticle(Guid? articleId, string title, string content, string introContent, Guid[] categoryIds)
        {
            //id和内容和标题为空、未登录、登陆id不是这篇文章的拥有者、分类id不属于自己
            if (articleId == null || articleId == Guid.Empty || title == null || content == null || title.Trim() == "" || content.Trim() == "")//提交的信息为空
            {
                return Json(new { status = "fail", result = "提交的信息不完整，请重试" }, JsonRequestBehavior.AllowGet);//返回错误信息
            }
            //获取当前登陆的id，cookie的id需要解密
            string userCookieId = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userCookieId, out message))
                {
                    return Json(new { status = "fail", result = message }, JsonRequestBehavior.AllowGet);//返回错误信息
                }
            }
            string userId = Session["userId"] == null ? userCookieId : Session["userId"].ToString();
            if (userId == null || userId.Trim() == "")//用户未登录
            {
                return Json(new { status = "fail", result = "获取不到用户信息，请检查登陆状态" }, JsonRequestBehavior.AllowGet);//返回错误信息
            }
            //查找提交的文章id是否存在，是否属于当前登陆用户
            IArticleManager articleManager = new ArticleManager();
            if (!await articleManager.ExistsArticle(articleId.Value))//文章id找不到则跳转文章不存在错误页面
            {
                return Json(new { status = "fail", result = "未能找到对应ID的文章，请稍后再试" }, JsonRequestBehavior.AllowGet);//返回错误信息
            }
            var data = await articleManager.GetOneArticleById(articleId.Value);//要经过上面的判断否则会出错
            if (data.userId != Guid.Parse(userId))//文章作者才可编辑文章
            {
                return Json(new { status = "fail", result = "非本人文章不可进行编辑" }, JsonRequestBehavior.AllowGet);//返回错误信息
            }
            //提交的分类信息不为空，循环自己所有的分类，对比是否正确
            if (categoryIds != null)
            {
                List<BlogCategoryDto> categoryDtoes = await articleManager.GetAllCategories(Guid.Parse(userId));//获取分类对象集合
                List<Guid> currentUserCategoryIds = new List<Guid>();//将分类对象中的分类id整合进一个集合中
                foreach (BlogCategoryDto category in categoryDtoes)
                {
                    currentUserCategoryIds.Add(category.Id);
                }
                for (int i = 0; i < categoryIds.Length; i++)//循环检查提交的分类id是否和自身的分类id有对应
                {
                    if (!currentUserCategoryIds.Contains(categoryIds[i]))//如果提交的分类id与自身的分类id没有匹配项，提示错误
                    {
                        return Json(new { status = "fail", result = "提交的分类与用户所拥有的分类不匹配，请重试！" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            await articleManager.EditArticle(articleId.Value, title, content, introContent, categoryIds);
            return Json(new { status = "ok", result = "提交成功！", articleId = articleId.Value }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> EditCategory(Guid categoryId, string newCategoryName)
        {
            //未登陆、系统内置、重名、信息为空、当前用户不是分类的拥有者不可编辑
            if (categoryId == null || newCategoryName == null || categoryId == Guid.Empty || newCategoryName.Trim() == "")
            {
                return Json(new { status = "fail", result = "提交的数据不完整，请重试！" }, JsonRequestBehavior.AllowGet);
            }
            //获取当前登陆的id，cookie的id需要解密
            string userCookieId = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userCookieId, out message))
                {
                    return Json(new { status = "fail", result = message }, JsonRequestBehavior.AllowGet);
                }
            }
            string userId = Session["userId"] == null ? userCookieId : Session["userId"].ToString();
            if (userId.Trim() == "")
            {
                return Json(new { status = "fail", result = "还未登陆无法编辑" }, JsonRequestBehavior.AllowGet);
            }
            IArticleManager articleManager = new ArticleManager();
            if (!await articleManager.ExistsCategory(categoryId))//分类id不存在
            {
                return Json(new { status = "fail", result = "未能找到对应ID的分类，请稍后再试" }, JsonRequestBehavior.AllowGet);
            }
            var data = await articleManager.GetOneCategoryById(categoryId);//要经过上面的判断否则会出错
            if (data.UserId != Guid.Parse(userId))//分类作者才可编辑分类
            {
                if (data.UserId == Guid.Parse("00000000-0000-0000-0000-000000000001"))
                {
                    return Json(new { status = "fail", result = "系统内置分类不可进行编辑" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = "fail", result = "非本人分类不可进行编辑" }, JsonRequestBehavior.AllowGet);
                }
            }
            //循环自己所有的分类，对比是否有重名
            List<BlogCategoryDto> categories = await articleManager.GetAllCategories(Guid.Parse(userId));
            foreach (BlogCategoryDto category in categories)
            {
                if (category.BlogCategoryName == newCategoryName)//修改后的名字和现有的重复，则提示失败
                {
                    return Json(new { status = "fail", result = "该名字已存在，请修改后重试！" }, JsonRequestBehavior.AllowGet);
                }
            }
            await articleManager.EditCategory(categoryId, newCategoryName);
            return Json(new { status = "ok", result = "编辑成功！" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> GoodCount(Guid id)
        {
            string userid = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userid, out message))
                {
                    return Json(new { result = message, status = "fail" });
                }
            }
            string userId = Session["userId"] == null ? userid : Session["userId"].ToString();
            if (userId == "")
            {
                return Json(new { result = "尚未登陆无法操作！", status = "fail" });
            }
            IArticleManager articleManager = new ArticleManager();
            await articleManager.GoodCountAdd(id, Guid.Parse(userId));
            ArticleDto data = await articleManager.GetOneArticleById(id);
            return Json(new { status = "ok", goodCount = data.GoodCount, badCount = data.BadCount });
        }

        [HttpPost]
        public async Task<ActionResult> BadCount(Guid id)
        {
            string userid = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userid, out message))
                {
                    return Json(new { result = message, status = "fail" });
                }
            }
            string userId = Session["userId"] == null ? userid : Session["userId"].ToString();
            if (userId == "")
            {
                return Json(new { result = "尚未登陆无法操作！", status = "fail" });
            }
            IArticleManager articleManager = new ArticleManager();
            await articleManager.BadCountAdd(id, Guid.Parse(userId));
            ArticleDto data = await articleManager.GetOneArticleById(id);
            return Json(new { status = "ok", goodCount = data.GoodCount, badCount = data.BadCount });
        }

        /// <summary>
        /// 获取文章的喜欢和不喜欢数量（需要登陆，因为要对已经点击过进行ui限制）
        /// </summary>
        /// <param name="id">文章id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetLikeHate(Guid id)
        {
            string userid = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userid, out message))
                {
                    return Json(new { result = message, status = "fail" });
                }
            }
            string userId = Session["userId"] == null ? userid : Session["userId"].ToString();
            if (userId == "")
            {
                return Json(new { result = "尚未登陆无法操作！", status = "fail" }, JsonRequestBehavior.AllowGet);
            }
            IArticleManager articleManager = new ArticleManager();
            string result = await articleManager.GetLikeHate(id, Guid.Parse(userId));
            return Json(new { status = "ok", result }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> AddComment(CreateCommentViewModel model)
        {
            string userid = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userid, out message))
                {
                    return Json(new { result = message, status = "fail" });
                }
            }
            string userId = Session["userId"] == null ? userid : Session["userId"].ToString();
            if (userId == "")
            {
                return Json(new { result = "尚未登陆无法操作！", status = "fail" });
            }
            IArticleManager articleManager = new ArticleManager();
            await articleManager.CreateComment(model.Id, Guid.Parse(userId), model.Content);
            return Json(new { status = "ok" });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteArticle(Guid? Id)
        {
            //获取当前登陆的id，cookie的id需要解密
            string userCookieId = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userCookieId, out message))
                {
                    return Json(new { status = "fail", result = message }, JsonRequestBehavior.AllowGet);//返回错误信息
                }
            }
            string userId = Session["userId"] == null ? userCookieId : Session["userId"].ToString();
            if (userId == null || userId.Trim() == "")//用户未登录
            {
                return Json(new { status = "fail", result = "获取不到用户信息，请检查登陆状态" }, JsonRequestBehavior.AllowGet);//返回错误信息
            }
            IArticleManager articleManager = new ArticleManager();
            if (Id == null || !await articleManager.ExistsArticle(Id.Value))//文章id找不到则跳转文章不存在错误页面
            {
                return Json(new { status = "fail", result = "未能找到对应ID的文章，请稍后再试" });//返回错误信息
            }//要经过上面的判断否则会出错
            var data = await articleManager.GetOneArticleById(Id.Value);
            if (data.userId != Guid.Parse(userId))//文章作者才可删除文章
            {
                return Json(new { status = "fail", result = "非本人文章不可进行删除" });//返回错误信息
            }
            await articleManager.RemoveArticle(Id.Value);
            return Json(new { status = "ok", result = "删除成功！" });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteCategory(Guid? Id)
        {
            //获取当前登陆的id，cookie的id需要解密
            string userCookieId = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                if (!JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userCookieId, out message))
                {
                    return Json(new { status = "fail", result = message }, JsonRequestBehavior.AllowGet);//返回错误信息
                }
            }
            string userId = Session["userId"] == null ? userCookieId : Session["userId"].ToString();
            if (userId == null || userId.Trim() == "")//用户未登录
            {
                return Json(new { status = "fail", result = "获取不到用户信息，请检查登陆状态" }, JsonRequestBehavior.AllowGet);//返回错误信息
            }
            IArticleManager articleManager = new ArticleManager();
            if (Id == null || !await articleManager.ExistsCategory(Id.Value))//文章id找不到则跳转分类不存在错误页面
            {
                return Json(new { status = "fail", result = "未能找到对应ID的分类，请稍后再试" });//返回错误信息
            }//要经过上面的判断否则会出错
            BlogCategory data = await articleManager.GetOneCategoryById(Id.Value);//获取分类id的所有信息
            if (data.UserId != Guid.Parse(userId))//文章作者才可删除文章
            {
                if (data.UserId == Guid.Parse("00000000-0000-0000-0000-000000000001"))//如果是01用户，说明是系统内置分类，不可删除
                {
                    return Json(new { status = "fail", result = "系统内置分类不可进行删除" });//返回错误信息
                }
                else
                {
                    return Json(new { status = "fail", result = "非本人分类不可进行删除" });//返回错误信息
                }
            }
            if (await articleManager.GetArticleDataCount(Guid.Parse(userId), Id.Value) != 0)
            {
                return Json(new { status = "fail", result = "已有文章引用了该分类，如需删除请先编辑文章取消引用该分类" });//返回错误信息
            }
            await articleManager.RemoveCategory(Id.Value);
            return Json(new { status = "ok", result = "删除成功！" });
        }

        /// <summary>
        /// 获取随机看看文章
        /// </summary>
        /// <param name="returnCount">返回数据数量</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetArticles(int returnCount)
        {
            IArticleManager articleManager = new ArticleManager();
            //获取所有文章的数量
            int allArticleCount = await articleManager.GetAllArticleDataCount();
            //random（ 0 - 所有的数量/数量 ）
            int maxCount = allArticleCount / returnCount;//最大值（不加1，最后一页可能不够数量）
            Random rd = new Random();
            int index = rd.Next(0, maxCount);
            //获取所有文章（random，数量）
            List<ArticleDto> data = await articleManager.GetAllArticles(index, returnCount);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetComments(Guid? id, int pageIndex, int pageSize)
        {
            var articleManager = new ArticleManager();
            if (id == null || !await articleManager.ExistsArticle(id.Value))
            {
                return Json(new { status = "fail", result = "未能找到对应文章ID的评论，请稍后再试" }, JsonRequestBehavior.AllowGet);
            }
            //查找所有评论
            List<CommentDto> data = await articleManager.GetCommentByArticleId(id.Value, pageIndex - 1, pageSize);
            int commentCount = await articleManager.GetCommentCountByArticleId(id.Value);
            int pageCount = commentCount % pageSize == 0 ? commentCount / pageSize : commentCount / pageSize + 1;//总页数
            return Json(new { status = "ok", pageCurrentIndex = pageIndex, pageSize, pageCount, pageMatchCommentCount = commentCount, data }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetMoreCategories(Guid? userId)
        {
            IUserManager userManager = new UserManager();
            if (userId == null || !await userManager.ExistsUser(userId.Value))
            {
                return Json(new { status = "fail", result = "获取用户信息失败！" }, JsonRequestBehavior.AllowGet);
            }
            var articleManager = new ArticleManager();
            List<BlogCategoryDto> data = await articleManager.GetAllCategories(userId.Value);
            return Json(new { status = "ok", data, userId }, JsonRequestBehavior.AllowGet);
        }
    }
}