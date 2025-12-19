namespace Northwind.EntityModels;

[Table("Categories")]
public class Category : IHaveTimestamps
{
    [Key]
    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(NorthwindConstants.CategoryNameMaxLength)]
    public string CategoryName { get; set; } = string.Empty;

    [Column(TypeName = "ntext")]
    public string? Description { get; set; }

    [Column(TypeName = "image")]
    public byte[]? Picture { get; set; }

    public virtual ICollection<Product> Products { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public override string ToString() => CategoryName;
}
