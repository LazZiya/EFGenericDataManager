using LazZiya.EFGenericDataManager.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LazZiya.EFGenericDataManager
{
    public interface IEFGenericDataManager
    {
        Task<bool> AddAsync<T>(T entity) where T : class;
        Task<bool> DeleteAsync<T>(T entity);
        Task<T> GetAsync<T, TKey>(TKey id)
            where T : class
            where TKey : IEquatable<TKey>;
        Task<T> GetAsync<T>(Expression<Func<T, bool>> searchExpression) where T : class;
        Task<bool> IsDuplicate<T>(Expression<Func<T, bool>> expression) where T : class;
        Task<(ICollection<T>, int)> ListAsync<T>(int start, int count, List<Expression<Func<T, bool>>> searchExpressions, List<OrderByExpression<T>> orderBy) where T : class;
        Task<bool> SetAsDefault<T, TKey>(TKey id)
            where T : class, IHasId<TKey>, IDefault
            where TKey : IEquatable<TKey>;
        Task<bool> UpdateAsync<T, TKey>(T entity)
            where T : class, IHasId<TKey>
            where TKey : IEquatable<TKey>;
        Task<bool> UpdateAsync<T>(T entity) where T : class;
    }
}