using FluentValidation;
using PropertiesService.Dtos;

namespace PropertiesService.Validators;

public class CreatePropertyRequestValidator : AbstractValidator<CreatePropertyRequest>
{
    public CreatePropertyRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Address).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.Country).NotEmpty();
        RuleFor(x => x.PropertyType).Must(v => v is "Casa" or "Departamento" or "Terreno");
        RuleFor(x => x.TransactionType).Must(v => v is "Venta" or "Alquiler");
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Area).GreaterThan(0);
        RuleFor(x => x.BuiltArea).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Bedrooms).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Photos).NotNull();
    }
}