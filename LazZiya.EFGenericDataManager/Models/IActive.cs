namespace LazZiya.EFGenericDataManager.Models
{
    /// <summary>
    /// Interface to implement an entity with boolean value to be marked as active/disabled
    /// </summary>
    public interface IActive
    {
        /// <summary>
        /// Get or set a value if the entity is active/disabled
        /// </summary>
        bool IsActive { get; set; }
    }
}
