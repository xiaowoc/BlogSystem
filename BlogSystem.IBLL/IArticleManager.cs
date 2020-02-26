using BlogSystem.DTO;
using BlogSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogSystem.IBLL
{
    public interface IArticleManager
    {
        Task<Guid> CreateArticle(string title, string content, string introContent,Guid[] categroyIds, Guid userId);

        Task CreateCategory(string name, Guid userId);

        Task<List<BlogCategoryDto>> GetAllCategories(Guid userId);

        Task<List<BlogCategoryDto>> GetCategoriesByCount(Guid userId, int count);

        Task<List<ArticleDto>> GetAllArticlesByUserId(Guid userId, int pageIndex, int pageSize);

        Task<List<ArticleDto>> GetAllArticlesByUserIdAndCategoryId(Guid userId, Guid categoryId, int pageIndex, int pageSize);

        Task<List<BlogCategoryDto>> GetAllCategoriesByUserId(Guid userId, int pageIndex, int pageSize);

        Task<int> GetArticleDataCount();

        Task<int> GetArticleDataCount(Guid id);

        Task<int> GetArticleDataCount(Guid userId, Guid categoryId);

        Task<int> GetSearchArticleDataCount(string searchWord);

        Task<int> GetCategoryDataCount(Guid id);

        Task<List<ArticleDto>> GetAllArticlesByEmail(string email);

        Task<List<ArticleDto>> GetAllArticlesByCategoryId(Guid categoryId);

        Task EditCategory(Guid categoryId, string newCategoryName);

        Task RemoveArticle(Guid articleId);

        Task RemoveCategory(Guid articleId);

        Task EditArticle(Guid articleId, string title, string content,string introContent, Guid[] categoryIds);

        Task<bool> ExistsArticle(Guid articleId);

        Task<bool> ExistsCategory(Guid categoryId);

        Task<ArticleDto> GetOneArticleById(Guid articleId);

        Task<BlogCategory> GetOneCategoryById(Guid categoryId);

        Task GoodCountAdd(Guid articleId, Guid userId);

        Task BadCountAdd(Guid articleId, Guid userId);

        Task<string> GetLikeHate(Guid articleId, Guid userId);

        Task CreateComment(Guid articleId, Guid userId, string content);

        Task<List<CommentDto>> GetCommentByArticleId(Guid articleId, int pageIndex, int pageSize);

        Task<int> GetCommentCountByArticleId(Guid articleId);

        Task<List<ArticleDto>> GetFamousArticle(int count);

        Task<List<ArticleDto>> GetCurrentUserFamousArticle(int count, Guid userId);

        Task<List<ArticleDto>> GetCurrentUserLatestArticle(int count, Guid userId, bool isTop);

        Task<int> GetAllArticleDataCount();

        Task<List<ArticleDto>> GetAllArticles(int pageIndex, int pageSize);

        Task<List<ArticleDto>> GetAllSearchArticles(string searchWord, int pageIndex, int pageSize);
    }
}
