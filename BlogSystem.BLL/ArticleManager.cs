using BlogSystem.DAL;
using BlogSystem.DTO;
using BlogSystem.IBLL;
using BlogSystem.IDAL;
using BlogSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlogSystem.BLL
{
    public class ArticleManager : IArticleManager
    {
        /// <summary>
        /// 反对
        /// </summary>
        /// <param name="articleId">文章id</param>
        /// <param name="userId">用户id</param>
        /// <returns></returns>
        public async Task BadCountAdd(Guid articleId, Guid userId)
        {
            int likeCount;
            int hateCount;
            using (ILikeHateService likeHateSvc = new LikeHateService())
            {
                string result = await GetLikeHate(articleId, userId);
                if (result == "none")//没有创建过
                {
                    LikeHate likeHate = new LikeHate() { ArticleId = articleId, UserId = userId, Like = false, Hate = true };
                    await likeHateSvc.CreatAsync(likeHate);
                }
                else if (result == "null")//创建了但都为false
                {
                    LikeHate likeHate = await likeHateSvc.GetAll().Where(m => m.ArticleId == articleId && m.UserId == userId && !m.Like && !m.Hate).FirstAsync();
                    likeHate.Hate = true;
                    await likeHateSvc.EditAsync(likeHate);
                }
                likeCount = await likeHateSvc.GetAll().Where(m => m.ArticleId == articleId && m.Like).CountAsync();
                hateCount = await likeHateSvc.GetAll().Where(m => m.ArticleId == articleId && m.Hate).CountAsync();
            }

            using (IArticleService articleSvc = new ArticleService())
            {
                var article = await articleSvc.GetOneByIdAsync(articleId);
                article.BadCount = hateCount;
                article.GoodCount = likeCount;
                await articleSvc.EditAsync(article);
            }
        }

        public async Task<string> GetLikeHate(Guid articleId, Guid userId)
        {
            using (ILikeHateService likeHateSvc = new LikeHateService())
            {
                var data = likeHateSvc.GetAll().Where(m => m.UserId == userId && m.ArticleId == articleId);
                if (await data.AnyAsync())//有存在记录
                {
                    //查询点赞了还是点踩了
                    var result = await data.FirstAsync();
                    if (result.Hate && result.Like)//两个都true错误，修改回未点赞和未点踩状态
                    {
                        result.Hate = false;
                        result.Like = false;
                        await likeHateSvc.EditAsync(result);
                        return "null";
                    }
                    else if (result.Like)//已经点赞,不可点踩
                    {
                        return "like";
                    }
                    else if (result.Hate)//已经点踩，不可点赞
                    {
                        return "hate";
                    }
                    else//其他情况
                    {
                        return "null";
                    }
                }
                else
                {
                    //没有记录当作没点赞也没点踩
                    return "none";
                }
            }
        }

        public async Task CreateArticle(string title, string content, Guid[] categroyIds, Guid userId)
        {
            using (IArticleService articleSvc = new ArticleService())
            {
                //处理内容
                string IntroContent = FilterHTML(content);
                Article article = new Article() { Title = title, Content = content, IntroContent = IntroContent, UserId = userId };
                await articleSvc.CreatAsync(article);//添加完会自动有Id值
                Guid articleId = article.Id;


                ////给简介表添加相同内容
                //using (IArticleIntroService articleintroSvc = new ArticleIntroService())
                //{
                //    //处理内容
                //    string articleIntroContent = FilterHTML(content);
                //    //创建实体
                //    ArticleIntro articleIntro = new ArticleIntro() { Title = article.Title, IntroContent = articleIntroContent, UserId = userId, Id = articleId, GoodCount = article.GoodCount, BadCount = article.BadCount, CreatTime = article.CreatTime, IsRemoved = article.IsRemoved };
                //    //创建
                //    await articleintroSvc.CreatAsync(articleIntro);
                //}

                using (IArticleToCategory articleToCategorySvc = new ArticleToCategoryService())
                {
                    if (categroyIds != null)//当分类不为空时才循环添加
                    {
                        foreach (var categroyId in categroyIds)
                        {
                            await articleToCategorySvc.CreatAsync(new ArticleToCategory()
                            {
                                ArticleId = articleId,
                                BlogCategoryId = categroyId
                            }, false);
                        }
                    }
                    await articleToCategorySvc.Save();
                }
            }
        }

        public async Task CreateCategory(string name, Guid userId)
        {
            using (IBlogCategory blogCategorySvc = new BlogCategoryService())
            {
                await blogCategorySvc.CreatAsync(new BlogCategory() { UserId = userId, CategoryName = name });
            }
        }

        public async Task EditArticle(Guid articleId, string title, string content, Guid[] categoryIds)
        {
            using (IArticleService articleSvc = new ArticleService())
            {
                //处理内容
                string IntroContent = FilterHTML(content);
                //获取当前文章的实体，修改标题和内容
                var article = await articleSvc.GetOneByIdAsync(articleId);
                article.Title = title;
                article.Content = content;
                article.IntroContent = IntroContent;
                await articleSvc.EditAsync(article);

                ////给简介表修改相同内容
                //using (IArticleIntroService articleintroSvc = new ArticleIntroService())
                //{
                //    //查找实体
                //    var articleIntro = await articleintroSvc.GetOneByIdAsync(articleId);
                //    //处理内容
                //    string articleIntroContent = FilterHTML(content);
                //    //修改实体内容
                //    articleIntro.IntroContent = articleIntroContent;
                //    //提交修改
                //    await articleintroSvc.EditAsync(articleIntro);
                //}

                using (IArticleToCategory articleToCategorySvc = new ArticleToCategoryService())
                {
                    //循环删除一篇文章的多个分类
                    foreach (var articleToCategory in articleToCategorySvc.GetAll().Where(m => m.ArticleId == articleId))
                    {
                        await articleToCategorySvc.DeleteAsync(articleToCategory, false);
                    }
                    //循环创建一篇文章多个分类
                    if (categoryIds != null)//当分类不为空时才循环添加
                    {
                        foreach (var categoryId in categoryIds)
                        {
                            await articleToCategorySvc.CreatAsync(new ArticleToCategory()
                            {
                                ArticleId = articleId,
                                BlogCategoryId = categoryId
                            }, false);
                        }
                    }
                    await articleToCategorySvc.Save();
                }
            }
        }

        public async Task EditCategory(Guid categoryId, string newCategoryName)
        {
            using (IBlogCategory blogCategorySvc = new BlogCategoryService())
            {
                //获取当前分类的实体，修改分类名字
                var category = await blogCategorySvc.GetOneByIdAsync(categoryId);
                category.CategoryName = newCategoryName;
                await blogCategorySvc.EditAsync(category);
            }
        }

        public async Task<bool> ExistsArticle(Guid articleId)
        {
            using (IArticleService articleSvc = new ArticleService())
            {
                return await articleSvc.GetAll().AnyAsync(m => m.Id == articleId);
            }
        }

        public async Task<bool> ExistsCategory(Guid categoryId)
        {
            using (IBlogCategory blogcategorySvc = new BlogCategoryService())
            {
                return await blogcategorySvc.GetAll().AnyAsync(m => m.Id == categoryId);
            }
        }

        public Task<List<ArticleDto>> GetAllArticlesByCategoryId(Guid categoryId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ArticleDto>> GetAllArticlesByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ArticleDto>> GetAllArticlesByUserId(Guid userId, int pageIndex, int pageSize)
        {
            using (IArticleService articleSvc = new ArticleService())
            {
                var list = await articleSvc.GetAllByPageOrder(pageSize, pageIndex, m => m.UserId == userId, false).Include(m => m.User).Select(m => new ArticleDto()
                {
                    Title = m.Title,
                    GoodCount = m.GoodCount,
                    BadCount = m.BadCount,
                    Email = m.User.Email,
                    Content = m.Content,
                    CreateTime = m.CreatTime,
                    Id = m.Id,
                    imagePath = m.User.ImagePath
                }).ToListAsync();

                using (IArticleToCategory articleToCategorySvc = new ArticleToCategoryService())
                {
                    foreach (var articleDto in list)
                    {
                        var cates = await articleToCategorySvc.GetAll().Include(m => m.BlogCategory).Where(m => m.ArticleId == articleDto.Id).ToListAsync();
                        articleDto.CategoryIds = cates.Select(m => m.BlogCategoryId).ToArray();
                        articleDto.CategoryNames = cates.Select(m => m.BlogCategory.CategoryName).ToArray();
                    }
                    return list;
                }
            }
        }

        public async Task<List<BlogCategoryDto>> GetAllCategoriesByUserId(Guid userId, int pageIndex, int pageSize)
        {
            using (IBlogCategory blogCategorySvc = new BlogCategoryService())
            {
                return await blogCategorySvc.GetAllByPageOrder(pageSize, pageIndex, m => m.UserId == userId, false).Select(m => new BlogCategoryDto()
                {
                    Id = m.Id,
                    BlogCategoryName = m.CategoryName
                }).ToListAsync();
            }
        }

        public async Task<List<BlogCategoryDto>> GetAllCategories(Guid userId)
        {
            using (IBlogCategory blogCategorySvc = new BlogCategoryService())
            {
                return await blogCategorySvc.GetAll().Where(m => m.UserId == userId).Select(m => new BlogCategoryDto()
                {
                    Id = m.Id,
                    BlogCategoryName = m.CategoryName
                }).ToListAsync();
            }
        }

        public async Task<int> GetArticleDataCount(Guid id)
        {
            using (IArticleService articleSvc = new ArticleService())
            {
                return await articleSvc.GetAll().CountAsync(m => m.UserId == id);
            }
        }

        public async Task<int> GetSearchArticleDataCount(string searchWord)
        {
            using (IArticleService articleSvc = new ArticleService())
            {
                return await articleSvc.GetAll().CountAsync(m => m.Title.Contains(searchWord) || m.User.Email.Contains(searchWord));
            }
        }

        public async Task<int> GetCategoryDataCount(Guid id)
        {
            using (IBlogCategory blogCategorySvc = new BlogCategoryService())
            {
                return await blogCategorySvc.GetAll().CountAsync(m => m.UserId == id);
            }
        }

        public async Task<ArticleDto> GetOneArticleById(Guid articleId)
        {
            using (IArticleService articleSvc = new ArticleService())
            {
                var data = await articleSvc.GetAll().Where(m => m.Id == articleId).Include(m => m.User).Select(m => new ArticleDto()
                {
                    Id = m.Id,
                    Title = m.Title,
                    Content = m.Content,
                    CreateTime = m.CreatTime,
                    Email = m.User.Email,
                    userId = m.UserId,
                    imagePath = m.User.ImagePath,
                    GoodCount = m.GoodCount,
                    BadCount = m.BadCount
                }).FirstAsync();
                using (IArticleToCategory articleToCategorySvc = new ArticleToCategoryService())
                {
                    var cates = await articleToCategorySvc.GetAll().Include(m => m.BlogCategory).Where(m => m.ArticleId == data.Id).ToListAsync();
                    data.CategoryIds = cates.Select(m => m.BlogCategoryId).ToArray();
                    data.CategoryNames = cates.Select(m => m.BlogCategory.CategoryName).ToArray();
                    return data;
                }
            }
        }

        /// <summary>
        /// 点赞
        /// </summary>
        /// <param name="articleId">文章id</param>
        /// <param name="userId">用户id</param>
        /// <returns></returns>
        public async Task GoodCountAdd(Guid articleId, Guid userId)
        {
            int likeCount;
            int hateCount;
            using (ILikeHateService likeHateSvc = new LikeHateService())
            {
                string result = await GetLikeHate(articleId, userId);
                if (result == "none")//没有创建过
                {
                    LikeHate likeHate = new LikeHate() { ArticleId = articleId, UserId = userId, Like = true, Hate = false };
                    await likeHateSvc.CreatAsync(likeHate);
                }
                else if (result == "null")//创建了但都为false
                {
                    LikeHate likeHate = await likeHateSvc.GetAll().Where(m => m.ArticleId == articleId && m.UserId == userId && !m.Like && !m.Hate).FirstAsync();
                    likeHate.Like = true;
                    await likeHateSvc.EditAsync(likeHate);
                }
                likeCount = await likeHateSvc.GetAll().Where(m => m.ArticleId == articleId && m.Like).CountAsync();
                hateCount = await likeHateSvc.GetAll().Where(m => m.ArticleId == articleId && m.Hate).CountAsync();
            }

            using (IArticleService articleSvc = new ArticleService())
            {
                var article = await articleSvc.GetOneByIdAsync(articleId);
                article.BadCount = hateCount;
                article.GoodCount = likeCount;
                await articleSvc.EditAsync(article);
            }
        }


        public async Task RemoveArticle(Guid articleId)
        {
            using (IArticleService articleSvc = new ArticleService())
            {
                Article article = await articleSvc.GetOneByIdAsync(articleId);
                await articleSvc.DeleteAsync(article);
            }
        }

        public async Task RemoveCategory(Guid categoryId)
        {
            using (IBlogCategory blogcategorySvc = new BlogCategoryService())
            {
                BlogCategory blogcategory = await blogcategorySvc.GetOneByIdAsync(categoryId);
                await blogcategorySvc.DeleteAsync(blogcategory);
            }
        }

        public async Task<BlogCategory> GetOneCategoryById(Guid categoryId)
        {
            using (IBlogCategory blogcategorySvc = new BlogCategoryService())
            {
                return await blogcategorySvc.GetOneByIdAsync(categoryId);
            }
        }

        /// <summary>
        /// 创建评论
        /// </summary>
        /// <param name="articleId"></param>
        /// <param name="userId"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task CreateComment(Guid articleId, Guid userId, string content)
        {
            using (ICommentService commentSvc = new CommentService())
            {
                await commentSvc.CreatAsync(new Comment()
                {
                    ArticleId = articleId,
                    UserId = userId,
                    Content = content
                });
            }
        }

        /// <summary>
        /// 根据文章ID查找评论
        /// </summary>
        /// <param name="articleId">文章id</param>
        /// <returns></returns>
        public async Task<List<CommentDto>> GetCommentByArticleId(Guid articleId, int pageIndex, int pageSize)
        {
            using (ICommentService commentSvc = new CommentService())
            {
                return await commentSvc.GetAllByPageOrder(pageSize, pageIndex, m => m.ArticleId == articleId, false).Include(m => m.User).Select(m => new CommentDto()
                {
                    Id = m.Id,
                    ArticleId = m.ArticleId,
                    UserId = m.UserId,
                    Content = m.Content,
                    CreateTime = m.CreatTime,
                    Email = m.User.Email,
                    ImagePath = m.User.ImagePath
                }).ToListAsync();
            }
        }

        /// <summary>
        /// 根据文章ID查找评论数量
        /// </summary>
        /// <param name="articleId">文章id</param>
        /// <returns></returns>
        public async Task<int> GetCommentCountByArticleId(Guid articleId)
        {
            using (ICommentService commentSvc = new CommentService())
            {
                return await commentSvc.GetAll().Where(m => m.ArticleId == articleId).CountAsync();
            }
        }

        /// <summary>
        /// 查找最热门的文章
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns></returns>
        public async Task<List<ArticleDto>> GetFamousArticle(int count)
        {
            using (IArticleService articleService = new ArticleService())
            {
                return await articleService.GetAll().OrderByDescending(m => m.GoodCount).Include(m => m.User).Select(m => new ArticleDto()
                {
                    Id = m.Id,
                    Title = m.Title,
                    Content = m.IntroContent,
                    CreateTime = m.CreatTime,
                    //Email = m.User.Email,
                    imagePath = m.User.ImagePath,
                    //GoodCount = m.GoodCount,
                    //BadCount = m.BadCount
                }).Take(count).ToListAsync();
            }
        }

        /// <summary>
        /// 查找当前账户最热门的文章
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns></returns>
        public async Task<List<ArticleDto>> GetCurrentUserFamousArticle(int count, Guid userId)
        {
            using (IArticleService articleService = new ArticleService())
            {
                return await articleService.GetAll().Where(m => m.User.Id == userId).OrderByDescending(m => m.GoodCount).Include(m => m.User).Select(m => new ArticleDto()
                {
                    Id = m.Id,
                    Title = m.Title,
                    Content = m.IntroContent,
                    CreateTime = m.CreatTime,
                    //Email = m.User.Email,
                    imagePath = m.User.ImagePath,
                    //GoodCount = m.GoodCount,
                    //BadCount = m.BadCount
                }).Take(count).ToListAsync();
            }
        }

        /// <summary>
        /// 获取所有文章的总数
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetAllArticleDataCount()
        {
            using (IArticleService articleSvc = new ArticleService())
            {
                return await articleSvc.GetAll().CountAsync();
            }
        }

        /// <summary>
        /// 从所有文章中获取对应数量的文章
        /// </summary>
        /// <param name="pageIndex">索引</param>
        /// <param name="pageSize">数量</param>
        /// <returns></returns>
        public async Task<List<ArticleDto>> GetAllArticles(int pageIndex, int pageSize)
        {
            using (IArticleService articleSvc = new ArticleService())
            {
                var list = await articleSvc.GetAllByPageOrder(pageSize, pageIndex, m => m.Id == m.Id, false).Include(m => m.User).Select(m => new ArticleDto()
                {
                    Title = m.Title,
                    GoodCount = m.GoodCount,
                    BadCount = m.BadCount,
                    Email = m.User.Email,
                    Content = m.IntroContent,
                    CreateTime = m.CreatTime,
                    Id = m.Id,
                    imagePath = m.User.ImagePath
                }).ToListAsync();

                using (IArticleToCategory articleToCategorySvc = new ArticleToCategoryService())
                {
                    foreach (var articleDto in list)
                    {
                        var cates = await articleToCategorySvc.GetAll().Include(m => m.BlogCategory).Where(m => m.ArticleId == articleDto.Id).ToListAsync();
                        articleDto.CategoryIds = cates.Select(m => m.BlogCategoryId).ToArray();
                        articleDto.CategoryNames = cates.Select(m => m.BlogCategory.CategoryName).ToArray();
                    }
                    return list;
                }
            }
        }

        /// <summary>
        /// 从所有文章中查找符合查找内容和对应数量的文章
        /// </summary>
        /// <param name="pageIndex">索引</param>
        /// <param name="pageSize">数量</param>
        /// <returns></returns>
        public async Task<List<ArticleDto>> GetAllSearchArticles(string searchWord, int pageIndex, int pageSize)
        {
            using (IArticleService articleSvc = new ArticleService())
            {
                var list = await articleSvc.GetAllByPageOrder(pageSize, pageIndex, m => m.Title.Contains(searchWord) || m.User.Email.Contains(searchWord), false).Include(m => m.User).Select(m => new ArticleDto()
                {
                    Title = m.Title,
                    GoodCount = m.GoodCount,
                    BadCount = m.BadCount,
                    Email = m.User.Email,
                    Content = m.IntroContent,
                    CreateTime = m.CreatTime,
                    Id = m.Id,
                    imagePath = m.User.ImagePath,
                    userId = m.User.Id
                }).ToListAsync();

                using (IArticleToCategory articleToCategorySvc = new ArticleToCategoryService())
                {
                    foreach (var articleDto in list)
                    {
                        var cates = await articleToCategorySvc.GetAll().Include(m => m.BlogCategory).Where(m => m.ArticleId == articleDto.Id).ToListAsync();
                        articleDto.CategoryIds = cates.Select(m => m.BlogCategoryId).ToArray();
                        articleDto.CategoryNames = cates.Select(m => m.BlogCategory.CategoryName).ToArray();
                    }
                    return list;
                }
            }
        }

        #region 去除html标签
        public static string FilterHTML(string strHtml, int returnLength = 100)
        {
            string[] aryReg =
            {
                @"<script[^>]*?>.*?</script>",
                @"<img\s+[^>]*\s*src\s*=\s*([']?)(?<url>\S+)'?[^>]*>",
                @"<(\/\s*)?!?((\w+:)?\w+)(\w+(\s*=?\s*(([""'])(\\[""'tbnr]|[^\7])*?\7|\w+)|.{0})|\s)*?(\/\s*)?>",
                @"([\r\n])[\s]+",
                @"&(quot|#34);",
                @"&(amp|#38);",
                @"&(lt|#60);",
                @"&(gt|#62);",
                @"&(nbsp|#160);",
                @"&(iexcl|#161);",
                @"&(cent|#162);",
                @"&(pound|#163);",
                @"&(copy|#169);",
                @"&#(\d+);",
                @"-->",
                @"<!--.*\n"
             };

            string[] aryRep =
            {
                "",
                "[图片]",
                " ",
                "",
                "\"",
                "&",
                "<",
                ">",
                "   ",
                "\xa1",  //chr(161),
                "\xa2",  //chr(162),
                "\xa3",  //chr(163),
                "\xa9",  //chr(169),
                "",
                "\r\n",
                ""
             };

            string strOutput = strHtml;
            for (int i = 0; i < aryReg.Length; i++)
            {
                Regex regex = new Regex(aryReg[i], RegexOptions.IgnoreCase);
                strOutput = regex.Replace(strOutput, aryRep[i]);
            }
            //去除多余空格，合并剩一个
            strOutput = strOutput.Trim();
            Regex regex1 = new Regex("\\s+", RegexOptions.IgnoreCase);
            strOutput = regex1.Replace(strOutput, " ");
            //strOutput.Replace("<", "");
            //strOutput.Replace(">", "");
            //strOutput.Replace("\r\n", "");
            return strOutput.Length > returnLength ? strOutput.Substring(0, returnLength) : strOutput;
        }
        #endregion

    }
}

