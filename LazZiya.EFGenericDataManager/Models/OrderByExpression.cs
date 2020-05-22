using System;
using System.Linq.Expressions;

namespace LazZiya.EFGenericDataManager.Models
{
    /// <summary>
    /// Define orderby direction
    /// </summary>
    public enum OrderByDir
    {
        /// <summary>
        /// Asceding sort
        /// </summary>
        ASC,

        /// <summary>
        /// Desceding sort
        /// </summary>
        DESC
    }

    /// <summary>
    /// Define an orderby expression
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OrderByExpression<T>
        where T : class
    {
        /// <summary>
        /// Order by expression
        /// </summary>
        public Expression<Func<T, object>> Expression { get; set; }

        /// <summary>
        /// Order by dir
        /// </summary>
        public OrderByDir OrderByDir { get; set; } = OrderByDir.ASC;
    }
}
