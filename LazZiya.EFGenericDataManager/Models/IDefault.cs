namespace LazZiya.EFGenericDataManager.Models
{
    /// <summary>
    /// Interface to implement an entity with boolean value to be marked as Default
    /// </summary>
    public interface IDefault
    {
        /// <summary>
        /// Get or set a value if this is the default request culture
        /// </summary>
        bool IsDefault { get; set; }
    }
}
