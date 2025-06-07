using System.Collections.Generic;

namespace Lab10.Data
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T? GetById(int id);
        T Add(T entity);
        T Update(T entity);
        void Delete(int id);

        //ajax
        T? GetNext(int id);
        T? GetPrevious(int id);
    }
}
