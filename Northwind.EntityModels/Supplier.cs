namespace Northwind.EntityModels;

[Table("Suppliers")]
public class Supplier : IHaveTimestamps
{
    [Key]
    [Column("SupplierID")]
    public int SupplierId { get; set; }

    [Required]
    [StringLength(NorthwindConstants.CompanyNameMaxLength)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(NorthwindConstants.ContactNameMaxLength)]
    public string? ContactName { get; set; }

    [StringLength(30)]
    public string? ContactTitle { get; set; }

    [StringLength(NorthwindConstants.AddressMaxLength)]
    public string? Address { get; set; }

    [StringLength(NorthwindConstants.CityMaxLength)]
    public string? City { get; set; }

    [StringLength(NorthwindConstants.RegionMaxLength)]
    public string? Region { get; set; }

    [StringLength(NorthwindConstants.PostalCodeMaxLength)]
    public string? PostalCode { get; set; }

    [StringLength(NorthwindConstants.CountryMaxLength)]
    public string? Country { get; set; }

    [StringLength(NorthwindConstants.PhoneMaxLength)]
    public string? Phone { get; set; }

    [StringLength(NorthwindConstants.PhoneMaxLength)]
    public string? Fax { get; set; }

    [Column(TypeName = "ntext")]
    public string? HomePage { get; set; }

    public virtual ICollection<Product> Products { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    [NotMapped]
    public string FullAddress =>
        string.Join(", ", new[] { Address, City, Region, PostalCode, Country }
            .Where(x => !string.IsNullOrWhiteSpace(x)));

    public override string ToString() => CompanyName;
}
