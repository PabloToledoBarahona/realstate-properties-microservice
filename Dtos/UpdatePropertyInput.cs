namespace PropertiesService.Dtos;

public class UpdatePropertyInput
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PropertyType { get; set; }
    public string? TransactionType { get; set; }
    public decimal? Price { get; set; }
    public int? Area { get; set; }
    public int? BuiltArea { get; set; }
    public int? Bedrooms { get; set; }
    public string? Status { get; set; }
    public List<string>? Photos { get; set; }
}