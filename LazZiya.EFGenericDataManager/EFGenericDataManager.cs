using LazZiya.EFGenericDataManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LazZiya.EFGenericDataManager
{
    /// <summary>
    /// Generic CRUD manager
    /// </summary>
    public class EFGenericDataManager<TContext> : IEFGenericDataManager
        where TContext : DbContext
    {
        private readonly TContext Context;

        /// <summary>
        /// Initialize a new instance of EFGenericDataManager
        /// </summary>
        /// <param name="context"></param>
        public EFGenericDataManager(TContext context)
        {
            if(context == null)
            {
                throw new NullReferenceException(nameof(context));
            }

            Context = context;
        }

        /// <summary>
        /// Get entity from db. 
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <typeparam name="TKey">Entity key type</typeparam>
        /// <param name="id">Entity id</param>
        /// <returns></returns>
        public async Task<T> GetAsync<T, TKey>(TKey id)
            where T : class, IHasId<TKey>
            where TKey : IEquatable<TKey>
        {
            return await Context.Set<T>().AsNoTracking().SingleOrDefaultAsync(x => x.ID.Equals(id));
        }

        /// <summary>
        /// Get entity from db
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="searchExpression">search expression</param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(Expression<Func<T, bool>> searchExpression)
            where T : class
        {
            return await Context.Set<T>().AsNoTracking().SingleOrDefaultAsync(searchExpression);
        }

        /// <summary>
        /// Get entity from db with relevant childs
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="searchExpression">search expression</param>
        /// <param name="includes">List of expressions for items to be included</param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(Expression<Func<T, bool>> searchExpression, List<Expression<Func<T, object>>> includes)
            where T : class
        {
            return await Context.Set<T>().AsNoTracking()
                .IncludeList(includes)
                .SingleOrDefaultAsync(searchExpression);
        }

        /// <summary>
        /// Add a new entity to the db
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> AddAsync<T>(T entity)
            where T : class
        {
            Context.Attach(entity).State = EntityState.Added;

            return await Context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Update entity, it must be fetched with FindAsync method.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync<T, TKey>(T entity)
            where T : class, IHasId<TKey>
            where TKey : IEquatable<TKey>
        {
            // check if entity is being tracked
            var local = Context.Set<T>().Local.SingleOrDefault(x => x.ID.Equals(entity.ID));

            // if entity is tracked detach it from context
            if (local != null)
                Context.Entry<T>(local).State = EntityState.Detached;

            Context.Attach(entity).State = EntityState.Modified;

            return await Context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync<T>(T entity)
        {
            Context.Attach(entity).State = EntityState.Deleted;

            return await Context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Get entiity of type T by given expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<int> Count<T>(Expression<Func<T, bool>> expression)
            where T : class
        {
            return await Context.Set<T>().AsNoTracking().CountAsync(expression);
        }

        /// <summary>
        /// Set an entity IsDefault value to true, and all rest entitities to false
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey">type of entity ID</typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> SetAsDefault<T, TKey>(TKey id)
            where T : class, IHasId<TKey>, IDefault, IActive
            where TKey : IEquatable<TKey>
        {
            // Set previous default to false
            var prevDefault = await GetAsync<T>(x => x.IsDefault == true);
            if (prevDefault != null)
            {
                prevDefault.IsDefault = false;
                var noOldDefault = await UpdateAsync<T, TKey>(prevDefault);

                if (!noOldDefault)
                    throw new Exception("A previous record with default value can't be set to false.");
            }

            // Set current entity to default
            var entity = await GetAsync<T, TKey>(id);
            if (entity != null)
            {
                entity.IsDefault = true;
                entity.IsActive = true;
                return await UpdateAsync<T, TKey>(entity);
            }

            // If we get here, something went wrong
            throw new Exception("Something went wrong!");
        }

        /// <summary>
        /// Returns a list of an ordered entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="searchExpressions"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public async Task<(ICollection<T>, int)> ListAsync<T>(int start, int count, List<Expression<Func<T, bool>>> searchExpressions, List<OrderByExpression<T>> orderBy)
            where T : class
        {
            var totalRecords = await Context.Set<T>().AsNoTracking().WhereList(searchExpressions).CountAsync();
            if (totalRecords == 0)
                return (null, totalRecords);

            var query = await Context.Set<T>().AsNoTracking()
                .WhereList<T>(searchExpressions)
                .OrderByList(orderBy)
                .Skip((start - 1) * count).Take(count)
                .ToListAsync();

            return (query, totalRecords);
        }
        
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
        public async Task<(ICollection<U>, int)> ListAsync<T, U>(int start, int count, 
            List<Expression<Func<T, bool>>> searchExpressions, 
            List<OrderByExpression<T>> orderBy, 
            List<Expression<Func<T, object>>> includes,
            Expression<Func<T, U>> select)
            where T : class
        {
            var totalRecords = await Context.Set<T>().AsNoTracking().WhereList(searchExpressions).CountAsync();
            if (totalRecords == 0)
                return (null, totalRecords);

            var query = await Context.Set<T>().AsNoTracking()
                .IncludeList(includes)
                .WhereList<T>(searchExpressions)
                .OrderByList(orderBy)
                .Select(select)
                .Skip((start - 1) * count).Take(count)
                .ToListAsync();

            return (query, totalRecords);
        }
    }
}
