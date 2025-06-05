namespace PropertiesService.Dtos;

public class CreatePropertyRequest
{
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
    public List<string> Photos { get; set; } = new();
}