namespace PropertiesService.Models;

public class Property
{
    public Guid IdProperty { get; set; }
    public Guid IdUser { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string PropertyType { get; set; } = default!;
    public string TransactionType { get; set; } = default!;
    public decimal Price { get; set; }
    public int Area { get; set; }
    public int BuiltArea { get; set; }
    public int Bedrooms { get; set; }
    public string Status { get; set; } = "activa";
    public List<string> Photos { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}