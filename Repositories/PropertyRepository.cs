using Cassandra;
using PropertiesService.Dtos;
using PropertiesService.Models;
using PropertiesService.Services;

namespace PropertiesService.Repositories;

public class PropertyRepository
{
    private readonly Cassandra.ISession _session;

    public PropertyRepository(CassandraSessionFactory factory)
    {
        _session = factory.GetSession();
    }

    public async Task CreateAsync(Property property)
    {
        // Inserción principal
        var insertMain = _session.Prepare(@"
            INSERT INTO properties_by_id (
                id_property, id_user, title, description, address, city, country,
                property_type, transaction_type, price, area, built_area, bedrooms,
                status, photos, created_at, updated_at
            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        ");
        var boundMain = insertMain.Bind(
            property.IdProperty,
            property.IdUser,
            property.Title,
            property.Description,
            property.Address,
            property.City,
            property.Country,
            property.PropertyType,
            property.TransactionType,
            property.Price,
            property.Area,
            property.BuiltArea,
            property.Bedrooms,
            property.Status,
            property.Photos,
            property.CreatedAt,
            property.UpdatedAt
        );
        await _session.ExecuteAsync(boundMain);

        // Inserción secundaria para consulta por ciudad
        var insertByCity = _session.Prepare(@"
            INSERT INTO properties_by_city (
                city, id_property, id_user, title, description, address, country,
                property_type, transaction_type, price, area, built_area, bedrooms,
                status, photos, created_at, updated_at
            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        ");
        var boundByCity = insertByCity.Bind(
            property.City,
            property.IdProperty,
            property.IdUser,
            property.Title,
            property.Description,
            property.Address,
            property.Country,
            property.PropertyType,
            property.TransactionType,
            property.Price,
            property.Area,
            property.BuiltArea,
            property.Bedrooms,
            property.Status,
            property.Photos,
            property.CreatedAt,
            property.UpdatedAt
        );
        await _session.ExecuteAsync(boundByCity);
    }

    public async Task<IEnumerable<Property>> GetAllAsync()
    {
        var result = await _session.ExecuteAsync(new SimpleStatement("SELECT * FROM properties_by_id"));
        return MapRowsToProperties(result);
    }

    public async Task<IEnumerable<Property>> GetByCityAsync(string city)
    {
        var statement = _session.Prepare("SELECT * FROM properties_by_city WHERE city = ?");
        var bound = statement.Bind(city);
        var result = await _session.ExecuteAsync(bound);
        return MapRowsToProperties(result);
    }

    private IEnumerable<Property> MapRowsToProperties(RowSet rows)
    {
        return rows.Select(row => new Property
        {
            IdProperty = row.GetValue<Guid>("id_property"),
            IdUser = row.GetValue<Guid>("id_user"),
            Title = row.GetValue<string>("title"),
            Description = row.GetValue<string>("description"),
            Address = row.GetValue<string>("address"),
            City = row.GetValue<string>("city"),
            Country = row.GetValue<string>("country"),
            PropertyType = row.GetValue<string>("property_type"),
            TransactionType = row.GetValue<string>("transaction_type"),
            Price = row.GetValue<decimal>("price"),
            Area = row.GetValue<int>("area"),
            BuiltArea = row.GetValue<int>("built_area"),
            Bedrooms = row.GetValue<int>("bedrooms"),
            Status = row.GetValue<string>("status"),
            Photos = row.GetValue<List<string>>("photos"),
            CreatedAt = row.GetValue<DateTime>("created_at"),
            UpdatedAt = row.GetValue<DateTime>("updated_at")
        });
    }

    public async Task<bool> UpdateAsync(Guid idProperty, Guid idUser, UpdatePropertyInput input)
    {
        // Validar propiedad existente y pertenencia
        var select = "SELECT * FROM properties_by_id WHERE id_property = ?";
        var row = (await _session.ExecuteAsync(new SimpleStatement(select, idProperty))).FirstOrDefault();
        if (row == null || row.GetValue<Guid>("id_user") != idUser)
            return false;

        var updates = new List<string>();
        var values = new List<object>();

        if (input.Title != null) { updates.Add("title = ?"); values.Add(input.Title); }
        if (input.Description != null) { updates.Add("description = ?"); values.Add(input.Description); }
        if (input.Address != null) { updates.Add("address = ?"); values.Add(input.Address); }
        if (input.City != null) { updates.Add("city = ?"); values.Add(input.City); }
        if (input.Country != null) { updates.Add("country = ?"); values.Add(input.Country); }
        if (input.PropertyType != null) { updates.Add("property_type = ?"); values.Add(input.PropertyType); }
        if (input.TransactionType != null) { updates.Add("transaction_type = ?"); values.Add(input.TransactionType); }
        if (input.Price.HasValue) { updates.Add("price = ?"); values.Add(input.Price.Value); }
        if (input.Area.HasValue) { updates.Add("area = ?"); values.Add(input.Area.Value); }
        if (input.BuiltArea.HasValue) { updates.Add("built_area = ?"); values.Add(input.BuiltArea.Value); }
        if (input.Bedrooms.HasValue) { updates.Add("bedrooms = ?"); values.Add(input.Bedrooms.Value); }
        if (input.Status != null) { updates.Add("status = ?"); values.Add(input.Status); }
        if (input.Photos != null) { updates.Add("photos = ?"); values.Add(input.Photos); }

        updates.Add("updated_at = ?"); values.Add(DateTime.UtcNow);

        var query = $"UPDATE properties_by_id SET {string.Join(", ", updates)} WHERE id_property = ?";
        values.Add(idProperty);

        var stmt = new SimpleStatement(query, values.ToArray());
        await _session.ExecuteAsync(stmt);
        return true;
    }
}