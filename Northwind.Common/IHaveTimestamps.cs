namespace Northwind.Common;

/// <summary>
/// Audit timestamps for entities.
/// </summary>
public interface IHaveTimestamps
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}
