namespace PropertiesService.Dtos;

public class PropertyFilterInput
{
    public string? City { get; set; }
    public string? PropertyType { get; set; } // Casa, Departamento, etc.
    public string? TransactionType { get; set; } // Venta, Alquiler
    public string? Status { get; set; } // activa, reservada, vendida, cancelada
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinArea { get; set; }
    public int? MaxArea { get; set; }
}