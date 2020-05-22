using LazZiya.EFGenericDataManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LazZiya.EFGenericDataManager
{
    /// <summary>
    /// Extensin methods for GenericDbManager
    /// </summary>
    public static class GenericQueryExtensions
    {
        /// <summary>
        /// Converts a list of search expressions to IQueryable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predications"></param>
        /// <returns></returns>
        public static IQueryable<T> WhereList<T>(this IQueryable<T> source, List<Expression<Func<T, bool>>> predications)
            where T : class
        {
            if (predications != null)
            {
                foreach (var p in predications)
                {
                    source = source.Where(p);
                }
            }

            return source;
        }

        /// <summary>
        /// Converts a list of include expressions to IQueryable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public static IQueryable<T> IncludeList<T>(this IQueryable<T> source, List<Expression<Func<T, object>>> includes)
            where T : class
        {
            if (includes != null)
            {
                foreach (var i in includes)
                {
                    source = source.Include(i);
                }
            }

            return source;
        }

        /// <summary>
        /// Converts a list of sort expressions to IQueryable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        public static IQueryable<T> OrderByList<T>(this IQueryable<T> source, List<OrderByExpression<T>> orders)
            where T : class
        {
            if (orders != null)
            {
                var orderedSource = orders[0].OrderByDir == OrderByDir.ASC
                    ? source.OrderBy(orders[0].Expression)
                    : source.OrderByDescending(orders[0].Expression);

                for (int i = 1; i < orders.Count; i++)
                {
                    orderedSource = orders[i].OrderByDir == OrderByDir.ASC
                        ? orderedSource.ThenBy(orders[i].Expression)
                        : orderedSource.ThenByDescending(orders[i].Expression);
                }

                return orderedSource;
            }

            return source;
        }
    }
}
