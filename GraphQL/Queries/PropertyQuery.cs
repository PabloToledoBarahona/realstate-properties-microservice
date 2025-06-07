using System.Collections;
using HotChocolate;
using HotChocolate.Types;
using PropertiesService.Models;
using PropertiesService.Repositories;
using PropertiesService.GraphQL;
using HotChocolate.Authorization;
using System.Security.Claims;
using PropertiesService.Dtos;

namespace PropertiesService.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class PropertyQuery
{
    public async Task<IEnumerable<Property>> GetAllPropertiesAsync(
        [Service] PropertyRepository repository)
        => await repository.GetAllAsync();

    public async Task<IEnumerable<Property>> GetPropertiesByCityAsync(
    string city,
    [Service] PropertyRepository repository)
    => await repository.GetByCityAsync(city);

    [Authorize]
    public async Task<IEnumerable<Property>> GetMyPropertiesAsync(
    ClaimsPrincipal user,
    [Service] PropertyRepository repository)
    {
        var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new GraphQLException("No autorizado");

        return await repository.GetByUserAsync(Guid.Parse(userId));
    }

    [GraphQLName("getPropertiesByFilter")]
    public async Task<IEnumerable<Property>> GetPropertiesByFilterAsync(
    PropertyFilterInput input,
    [Service] PropertyRepository repository)
    {
        return await repository.GetByFilterAsync(input);
    }
}

