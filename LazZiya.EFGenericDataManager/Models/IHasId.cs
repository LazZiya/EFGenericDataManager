using System;

namespace LazZiya.EFGenericDataManager.Models
{
    /// <summary>
    /// Declare an interafce tha an entity should have an ID
    /// </summary>
    /// <typeparam name="TKey">key type</typeparam>
    public interface IHasId<TKey>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Entity ID
        /// </summary>
        TKey ID { get; set; }
    }
}
