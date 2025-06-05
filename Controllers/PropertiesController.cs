using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertiesService.Dtos;
using PropertiesService.Models;
using PropertiesService.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PropertiesService.Controllers;

[ApiController]
[Route("api/properties")]
public class PropertiesController : ControllerBase
{
    private readonly PropertyRepository _repository;

    public PropertiesController(PropertyRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateProperty([FromBody] CreatePropertyRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { message = "El token JWT no contiene un ID de usuario válido." });
        }

        var property = new Property
        {
            IdProperty = Guid.NewGuid(),
            IdUser = userId,
            Title = request.Title,
            Description = request.Description,
            Address = request.Address,
            City = request.City,
            Country = request.Country,
            PropertyType = request.PropertyType,
            TransactionType = request.TransactionType,
            Price = request.Price,
            Area = request.Area,
            BuiltArea = request.BuiltArea,
            Bedrooms = request.Bedrooms,
            Status = "activa",
            Photos = request.Photos,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(property);

        return Ok(new { message = "Propiedad publicada con éxito", property.IdProperty });
    }
}