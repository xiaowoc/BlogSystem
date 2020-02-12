using BlogSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BlogSystem.IDAL
{
    public interface IBaseService<T> : IDisposable where T : BaseEntity
    {
        Task CreatAsync(T model, bool saved = true);

        Task EditAsync(T model, bool saved = true);

        Task DeleteAsync(Guid id, bool saved = true);

        Task DeleteAsync(T model, bool saved = true);

        Task Save();

        Task<T> GetOneByIdAsync(Guid id);

        IQueryable<T> GetAll();

        IQueryable<T> GetAllByPage(int pageSize, int pageIndex, Expression<Func<T, bool>> predicate);

        IQueryable<T> GetAllOrder(bool asc = true);

        IQueryable<T> GetAllByPageOrder(int pageSize, int pageIndex, Expression<Func<T, bool>> predicate, bool asc = true);
    }
}
