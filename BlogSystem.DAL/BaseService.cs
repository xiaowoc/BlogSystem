using BlogSystem.IDAL;
using BlogSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BlogSystem.DAL
{
    public class BaseService<T> : IBaseService<T> where T : BaseEntity, new()
    {
        private readonly BlogContext db;

        public BaseService(BlogContext db)
        {
            this.db = db;
        }
        public async Task CreatAsync(T model, bool saved = true)
        {
            db.Set<T>().Add(model);
            if (saved) await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id, bool saved = true)
        {
            db.Configuration.ValidateOnSaveEnabled = false;
            T t = new T() { Id = id };
            db.Entry(t).State = EntityState.Unchanged;
            t.IsRemoved = true;
            if (saved)
            {
                await db.SaveChangesAsync();
                db.Configuration.ValidateOnSaveEnabled = true;
            }
        }

        public async Task DeleteAsync(T model, bool saved = true)
        {
            await DeleteAsync(model.Id, saved);
        }

        public async Task EditAsync(T model, bool saved = true)
        {
            db.Configuration.ValidateOnSaveEnabled = false;
            db.Entry(model).State = EntityState.Modified;
            if (saved)
            {
                await db.SaveChangesAsync();
                db.Configuration.ValidateOnSaveEnabled = true;
            }
        }

        /// <summary>
        /// 返回未被删除的数据（还没有执行）
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> GetAll()
        {
            return db.Set<T>().Where(m => !m.IsRemoved).AsNoTracking();
        }

        public IQueryable<T> GetAllByPage(int pageSize, int pageIndex, Expression<Func<T, bool>> predicate)
        {
            return GetAll().Where(predicate).Skip(pageSize * pageIndex).Take(pageSize);
        }

        public IQueryable<T> GetAllByPageOrder(int pageSize, int pageIndex, Expression<Func<T, bool>> predicate, bool asc = true)
        {
            return GetAllOrder(asc).Where(predicate).Skip(pageSize * pageIndex).Take(pageSize);
        }

        public IQueryable<T> GetAllOrder(bool asc = true)
        {
            var datas = GetAll();
            if (asc)
            {
                datas = datas.OrderBy(m => m.CreatTime);
            }
            else
            {
                datas = datas.OrderByDescending(m => m.CreatTime);
            }
            return datas;
        }

        public async Task<T> GetOneByIdAsync(Guid id)
        {
            return await GetAll().FirstAsync(m => m.Id == id);
        }

        public async Task Save()
        {
            await db.SaveChangesAsync();
            db.Configuration.ValidateOnSaveEnabled = true;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
