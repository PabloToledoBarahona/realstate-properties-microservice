using System.Collections;
using HotChocolate;
using HotChocolate.Types;
using PropertiesService.Models;
using PropertiesService.Repositories;
using PropertiesService.GraphQL;

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
}

