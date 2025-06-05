using HotChocolate;
using HotChocolate.Types;
using PropertiesService.Dtos;
using PropertiesService.Models;
using PropertiesService.Repositories;
using System.Security.Claims;
using PropertiesService.GraphQL;

namespace PropertiesService.GraphQL.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class PropertyMutation
{
    public async Task<Property> CreatePropertyAsync(
        CreatePropertyRequest input,
        ClaimsPrincipal user,
        [Service] PropertyRepository repository)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
            throw new GraphQLException("No autorizado");

        var property = new Property
        {
            IdProperty = Guid.NewGuid(),
            IdUser = Guid.Parse(userId),
            Title = input.Title,
            Description = input.Description,
            Address = input.Address,
            City = input.City,
            Country = input.Country,
            PropertyType = input.PropertyType,
            TransactionType = input.TransactionType,
            Price = input.Price,
            Area = input.Area,
            BuiltArea = input.BuiltArea,
            Bedrooms = input.Bedrooms,
            Status = "activa",
            Photos = input.Photos,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await repository.CreateAsync(property);
        return property;
    }
}