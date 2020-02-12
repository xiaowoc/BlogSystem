using BlogSystem.BLL;
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
    [BlogSystemAuth]
    public class ArticleController : Controller
    {
        [HttpGet]
        public ActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCategory(CreateCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                Guid userId = Guid.Parse(Session["userId"].ToString());
                IArticleManager articleManager = new ArticleManager();
                articleManager.CreateCategory(model.CategoryName, userId);
                return RedirectToAction("CategoryList", new { userId });
            }
            ModelState.AddModelError(string.Empty, "输入有误");
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> CategoryList(string userId, int pageIndex = 1, int pageSize = 7)
        {
            Guid currentUserId = Guid.Parse(Session["userId"].ToString());//获取当前登陆的id
            if (userId == null)
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
            ViewBag.IsCurrentUser = userIdGuid == currentUserId ? true : false;//是否为当前登陆用户
            ViewBag.RequestId = userIdGuid;//当前请求id
            return View(categorys);
        }

        [HttpGet]
        public async Task<ActionResult> CreateArticle()
        {
            var userId = Guid.Parse(Session["userId"].ToString());
            ViewBag.Categories = await new ArticleManager().GetAllCategories(userId);
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> CreateArticle(CreateArticleViewModel model)
        {
            var userId = Guid.Parse(Session["userId"].ToString());
            if (ModelState.IsValid)
            {
                await new ArticleManager().CreateArticle(model.Title, model.Content, model.CategoryIds, userId);
                return RedirectToAction("ArticleList", new { userId });
            }
            ViewBag.Categories = await new ArticleManager().GetAllCategories(userId);
            ModelState.AddModelError(string.Empty, "添加失败");
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> ArticleList(string userId, int pageIndex = 1, int pageSize = 7)
        {
            Guid currentUserId = Guid.Parse(Session["userId"].ToString());//获取当前登陆的id
            if (userId == null)
            {
                return RedirectToAction(nameof(ArticleList), new { userId = currentUserId });
            }
            //需要返回前端 总页数 当前页数 单个页面数量 
            var articleManager = new ArticleManager();
            IUserManager userManager = new UserManager();
            Guid userIdGuid;
            Guid.TryParse(userId, out userIdGuid);
            if (!await userManager.ExistsUser(userIdGuid) || userIdGuid == Guid.Empty)
            {
                ErrorController.message = "未能找到对应ID的用户，请稍后再试";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
            var articles = await articleManager.GetAllArticlesByUserId(userIdGuid, (pageIndex - 1), pageSize);//数据库是从0开始的
            var dataCount = await articleManager.GetArticleDataCount(userIdGuid);//文章总数
            ViewBag.PageCount = dataCount % pageSize == 0 ? dataCount / pageSize : dataCount / pageSize + 1;//总页数
            ViewBag.PageIndex = pageIndex;//当前页数
            ViewBag.PageSize = pageSize;//当前显示数目
            ViewBag.IsCurrentUser = userIdGuid == currentUserId ? true : false;//是否为当前登陆用户
            ViewBag.RequestId = userIdGuid;//当前请求id
            return View(articles);
        }

        [HttpGet]
        [AllowAnonymous]
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
            //查找用户粉丝数
            ViewBag.FansCount = user.FansCount;
            //用户id
            ViewBag.UserId = user.Id;
            //查找文章总数
            ViewBag.ArticleCount = await articleManager.GetArticleDataCount(user.Id);
            //查询是否已经关注
            string userid = ""; string message;
            if (Request.Cookies["userId"] != null)
            {
                JwtHelper.GetJwtDecode(Request.Cookies["userId"].Value, out userid, out message);
            }
            string userId = Session["userId"] == null ? userid : Session["userId"].ToString();
            ViewBag.IsFocused = userId == "" ? false : await userManager.IsFocused(Guid.Parse(userId), user.Id);//id为空也视为没关注
            return View(data);
        }


        [HttpGet]
        public async Task<ActionResult> EditArticle(Guid? id)
        {
            //获取当前文章实体
            IArticleManager articleManager = new ArticleManager();
            if (id == null || !await articleManager.ExistsArticle(id.Value))//文章id找不到则跳转文章不存在错误页面
            {
                ErrorController.message = "未能找到对应ID的文章，请稍后再试";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
            var data = await articleManager.GetOneArticleById(id.Value);//要经过上面的判断否则会出错
            Guid userId = Guid.Parse(Session["userId"].ToString());
            if (data.userId == userId)//文章作者才可编辑文章
            {
                //获取所有分类
                ViewBag.Categories = await new ArticleManager().GetAllCategories(userId);
                //将实体内容放进viewmodel中输出到前端
                return View(new EditArticleViewModel()
                {
                    Id = data.Id,
                    Title = data.Title,
                    Content = data.Content,
                    CategoryIds = data.CategoryIds
                });
            }
            else
            {
                ErrorController.message = "非本人文章不可进行编辑";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
        }

        [HttpGet]
        public async Task<ActionResult> EditCategory(Guid? id)
        {
            //获取当前分类实体
            IArticleManager articleManager = new ArticleManager();
            if (id == null || !await articleManager.ExistsCategory(id.Value))//分类id找不到则跳转分类不存在错误页面
            {
                ErrorController.message = "未能找到对应ID的分类，请稍后再试";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
            var data = await articleManager.GetOneCategoryById(id.Value);//要经过上面的判断否则会出错
            Guid userId = Guid.Parse(Session["userId"].ToString());
            if (data.UserId == userId)//文章作者才可编辑分类
            {
                //获取所有分类
                ViewBag.Categories = await new ArticleManager().GetAllCategories(userId);
                //将实体内容放进viewmodel中输出到前端
                return View(new EditCategoryViewModel()
                {
                    Id = data.Id,
                    CategoryName = data.CategoryName
                });
            }
            else
            {
                ErrorController.message = "非本人分类不可进行编辑";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditArticle(EditArticleViewModel model)
        {
            IArticleManager articleManager = new ArticleManager();
            if (!await articleManager.ExistsArticle(model.Id))//文章id找不到则跳转文章不存在错误页面
            {
                ErrorController.message = "未能找到对应ID的文章，请稍后再试";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
            var data = await articleManager.GetOneArticleById(model.Id);//要经过上面的判断否则会出错
            Guid userId = Guid.Parse(Session["userId"].ToString());
            if (data.userId == userId)//文章作者才可编辑文章
            {
                if (ModelState.IsValid)
                {
                    await articleManager.EditArticle(model.Id, model.Title, model.Content, model.CategoryIds);
                    return RedirectToAction("ArticleList", new { userId = userId });
                }
                else
                {
                    //获取所有分类
                    ViewBag.Categories = await new ArticleManager().GetAllCategories(userId);
                    return View(model);
                }
            }
            else
            {
                ErrorController.message = "非本人文章不可进行编辑";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCategory(EditCategoryViewModel model)
        {
            IArticleManager articleManager = new ArticleManager();
            if (!await articleManager.ExistsCategory(model.Id))//分类id找不到则跳转分类不存在错误页面
            {
                ErrorController.message = "未能找到对应ID的分类，请稍后再试";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
            var data = await articleManager.GetOneCategoryById(model.Id);//要经过上面的判断否则会出错
            Guid userId = Guid.Parse(Session["userId"].ToString());
            if (data.UserId == userId)//分类作者才可编辑分类
            {
                if (ModelState.IsValid)
                {
                    List<BlogCategoryDto> categories = await articleManager.GetAllCategories(userId);
                    foreach (BlogCategoryDto category in categories)
                    {
                        if (category.BlogCategoryName == model.CategoryName)//修改后的名字和现有的重复，则提示失败
                        {
                            ModelState.AddModelError(string.Empty, "该名字已存在，请修改后重试！");
                            return View(model);
                        }
                    }
                    await articleManager.EditCategory(model.Id, model.CategoryName);
                    return RedirectToAction("CategoryList", new { userId = userId });
                }
                else
                {
                    return View(model);
                }
            }
            else
            {
                ErrorController.message = "非本人分类不可进行编辑";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
        }

        [HttpPost]
        [AllowAnonymous]
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
            else
            {
                IArticleManager articleManager = new ArticleManager();
                await articleManager.GoodCountAdd(id, Guid.Parse(userId));
                ArticleDto data = await articleManager.GetOneArticleById(id);
                return Json(new { status = "ok", goodCount = data.GoodCount, badCount = data.BadCount });
            }
        }

        [HttpPost]
        [AllowAnonymous]
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
            else
            {
                IArticleManager articleManager = new ArticleManager();
                await articleManager.BadCountAdd(id, Guid.Parse(userId));
                ArticleDto data = await articleManager.GetOneArticleById(id);
                return Json(new { status = "ok", goodCount = data.GoodCount, badCount = data.BadCount });
            }
        }

        [HttpGet]
        [AllowAnonymous]
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
            else
            {
                IArticleManager articleManager = new ArticleManager();
                string result = await articleManager.GetLikeHate(id, Guid.Parse(userId));
                return Json(new { status = "ok", result }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [AllowAnonymous]
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
            else
            {
                IArticleManager articleManager = new ArticleManager();
                await articleManager.CreateComment(model.Id, Guid.Parse(userId), model.Content);
                return Json(new { status = "ok" });
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteArticle(Guid? Id)
        {
            IArticleManager articleManager = new ArticleManager();
            if (Id == null || !await articleManager.ExistsArticle(Id.Value))//文章id找不到则跳转文章不存在错误页面
            {
                ErrorController.message = "未能找到对应ID的文章，请稍后再试";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }//要经过上面的判断否则会出错
            var data = await articleManager.GetOneArticleById(Id.Value);
            Guid userId = Guid.Parse(Session["userId"].ToString());
            if (data.userId != userId)//文章作者才可删除文章
            {
                ErrorController.message = "非本人文章不可进行删除";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
            await articleManager.RemoveArticle(Id.Value);
            return Json(new { status = "ok" });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteCategory(Guid? Id)
        {
            IArticleManager articleManager = new ArticleManager();
            if (Id == null || !await articleManager.ExistsCategory(Id.Value))//文章id找不到则跳转分类不存在错误页面
            {
                ErrorController.message = "未能找到对应ID的分类，请稍后再试";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }//要经过上面的判断否则会出错
            BlogCategory data = await articleManager.GetOneCategoryById(Id.Value);//获取分类id的所有信息
            Guid userId = Guid.Parse(Session["userId"].ToString());
            if (data.UserId != userId)//文章作者才可删除文章
            {
                ErrorController.message = "非本人分类不可进行删除";
                return RedirectToAction("IllegalOperationError", "Error");//返回错误页面
            }
            await articleManager.RemoveCategory(Id.Value);
            return Json(new { status = "ok" });
        }

        /// <summary>
        /// 获取随机看看文章
        /// </summary>
        /// <param name="returnCount">返回数据数量</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
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
        [AllowAnonymous]
        public async Task<ActionResult> GetComments(Guid? id, int pageIndex, int pageSize)
        {
            var articleManager = new ArticleManager();
            if (id == null || !await articleManager.ExistsArticle(id.Value))
            {
                return Json(new { status = "未能找到对应文章ID的评论，请稍后再试" });
            }
            //查找所有评论
            List<CommentDto> data = await articleManager.GetCommentByArticleId(id.Value, pageIndex - 1, pageSize);
            int commentCount = await articleManager.GetCommentCountByArticleId(id.Value);
            int pageCount = commentCount % pageSize == 0 ? commentCount / pageSize : commentCount / pageSize + 1;//总页数
            return Json(new { status = "ok", pageCurrentIndex = pageIndex, pageCount, data }, JsonRequestBehavior.AllowGet);
        }
    }
}