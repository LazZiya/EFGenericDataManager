using LazZiya.EFGenericDataManager.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LazZiya.EFGenericDataManager
{
    /// <summary>
    /// Generic CRUD manager interface
    /// </summary>
    public interface IEFGenericDataManager
    {
        /// <summary>
        /// Get entity of type T by ID
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey">type of entity ID</typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T> GetAsync<T, TKey>(TKey id)
            where T : class, IHasId<TKey>
            where TKey : IEquatable<TKey>;

        /// <summary>
        /// Add entity of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<bool> AddAsync<T>(T entity) where T : class;

        /// <summary>
        /// Get entity from db with relevant childs
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="searchExpression">search expression</param>
        /// <param name="includes">List of expressions for items to be included</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(Expression<Func<T, bool>> searchExpression, List<Expression<Func<T, object>>> includes)
            where T : class;

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync<T>(T entity);

        /// <summary>
        /// Get entiity of type T by given expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="searchExpression"></param>
        /// <returns></returns>
        Task<T> GetAsync<T>(Expression<Func<T, bool>> searchExpression) where T : class;

        /// <summary>
        /// Get count of entities by given expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        Task<int> Count<T>(Expression<Func<T, bool>> expression) where T : class;

        /// <summary>
        /// Set entity of type T and given ID as default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey">type of entity ID</typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> SetAsDefault<T, TKey>(TKey id)
            where T : class, IHasId<TKey>, IDefault, IActive
            where TKey : IEquatable<TKey>;

        /// <summary>
        /// Get list of ordered entities by given search expression and ordering expressions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="searchExpressions"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        Task<(ICollection<T>, int)> ListAsync<T>(int start, int count, List<Expression<Func<T, bool>>> searchExpressions, List<OrderByExpression<T>> orderBy) where T : class;

        /// <summary>
        /// Update entity of type T, 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync<T, TKey>(T entity)
            where T : class, IHasId<TKey>
            where TKey : IEquatable<TKey>;

        /// <summary>
        /// Select a list of entities from type T and return as list of type U including related child entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U">return type</typeparam>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="searchExpressions"></param>
        /// <param name="orderBy"></param>
        /// <param name="includes">expression list for included items</param>
        /// <param name="select">expression to select properties and map to a new object</param>
        /// <returns></returns>
        Task<(ICollection<U>, int)> ListAsync<T, U>(int start, int count,
            List<Expression<Func<T, bool>>> searchExpressions,
            List<OrderByExpression<T>> orderBy,
            List<Expression<Func<T, object>>> includes,
            Expression<Func<T, U>> select)
            where T : class;
    }
}