namespace Northwind.EntityModels;

[Table("Products")]
public class Product : IHaveTimestamps
{
    [Key]
    [Column("ProductID")]
    public int ProductId { get; set; }

    [Required]
    [StringLength(NorthwindConstants.ProductNameMaxLength)]
    public string ProductName { get; set; } = string.Empty;

    [Column("SupplierID")]
    public int? SupplierId { get; set; }

    [Column("CategoryID")]
    public int? CategoryId { get; set; }

    [StringLength(20)]
    public string? QuantityPerUnit { get; set; }

    [Column(TypeName = "money")]
    public decimal? UnitPrice { get; set; }

    public short? UnitsInStock { get; set; }
    public short? UnitsOnOrder { get; set; }
    public short? ReorderLevel { get; set; }

    [Required]
    public bool Discontinued { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public virtual Category? Category { get; set; }

    [ForeignKey(nameof(SupplierId))]
    public virtual Supplier? Supplier { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Computed
    [NotMapped]
    public bool NeedsReorder => !Discontinued && UnitsInStock <= ReorderLevel;

    [NotMapped]
    public decimal InventoryValue => (UnitPrice ?? 0) * (UnitsInStock ?? 0);

    [NotMapped]
    public bool IsInStock => !Discontinued && UnitsInStock > 0;

    public void Discontinue()
    {
        Discontinued = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public override string ToString() => ProductName;
}
