using HotChocolate;
using HotChocolate.Types;
using PropertiesService.Dtos;
using PropertiesService.Models;
using PropertiesService.Repositories;
using System.Security.Claims;
using PropertiesService.GraphQL;

namespace PropertiesService.GraphQL.Mutations
{
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

        public async Task<bool> UpdatePropertyAsync(
            Guid id,
            UpdatePropertyInput input,
            ClaimsPrincipal user,
            [Service] PropertyRepository repository)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                throw new GraphQLException("No autorizado");

            return await repository.UpdateAsync(id, Guid.Parse(userId), input);
        }

        public async Task<bool> UpdatePropertyStatusAsync(
            Guid id,
            UpdatePropertyStatusInput input,
            ClaimsPrincipal user,
            [Service] PropertyRepository repository)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                throw new GraphQLException("No autorizado");

            var allowedStatuses = new[] { "activa", "reservada", "vendida", "cancelada" };
            if (!allowedStatuses.Contains(input.Status))
                throw new GraphQLException("Estado no válido");

            try
            {
                var success = await repository.UpdateStatusAsync(id, Guid.Parse(userId), input.Status);
                if (!success)
                    throw new GraphQLException("No se pudo actualizar el estado de la propiedad");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] UpdatePropertyStatusAsync: {ex}");
                throw new GraphQLException("Error inesperado al actualizar el estado de la propiedad");
            }
        }
    }
}