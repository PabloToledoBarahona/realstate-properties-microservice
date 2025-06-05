using Cassandra;
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
}