using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Core.Interfaces
{
    public interface IGenericRepository<T> where T :  class
    {
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAll(Expression<Func<T, bool>> whereCondition);

       
        T SingleOrDefault(Expression<Func<T, bool>> whereCondition);
        T GetById(object id);
        bool Exists(Expression<Func<T, bool>> whereCondition);
        void Insert(T entity);
        void Update(T entity);
        void Delete(object id);
    }
}
