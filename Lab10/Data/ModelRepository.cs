using Lab10.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Lab10.Data
{
    public class ModelRepository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public ModelRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IEnumerable<T> GetAll() => _dbSet.ToList();

        public T? GetById(int id) => _dbSet.Find(id);

        public T Add(T entity)
        {
            _dbSet.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public T Update(T entity)
        {
            _dbSet.Update(entity);
            _context.SaveChanges();
            return entity;
        }

        public void Delete(int id)
        {
            var entity = _dbSet.Find(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                _context.SaveChanges();
            }
        }

        public T? GetNext(int id)
        {
            return _dbSet.AsQueryable()
                .OrderBy(e => EF.Property<int>(e, "Id"))
                .FirstOrDefault(e => EF.Property<int>(e, "Id") > id);
        }

        public T? GetPrevious(int id)
        {
            return _dbSet.AsQueryable()
                .OrderByDescending(e => EF.Property<int>(e, "Id"))
                .FirstOrDefault(e => EF.Property<int>(e, "Id") < id);
        }
    }
}

