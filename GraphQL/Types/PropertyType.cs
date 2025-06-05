using HotChocolate.Types;
using PropertiesService.Models;

namespace PropertiesService.GraphQL.Types;

public class PropertyType : ObjectType<Property>
{
    protected override void Configure(IObjectTypeDescriptor<Property> descriptor)
    {
        descriptor.Field(x => x.IdProperty).Type<UuidType>();
        descriptor.Field(x => x.IdUser).Type<UuidType>();
        descriptor.Field(x => x.Title).Type<StringType>();
        descriptor.Field(x => x.Description).Type<StringType>();
        descriptor.Field(x => x.Address).Type<StringType>();
        descriptor.Field(x => x.City).Type<StringType>();
        descriptor.Field(x => x.Country).Type<StringType>();
        descriptor.Field(x => x.PropertyType).Type<StringType>();
        descriptor.Field(x => x.TransactionType).Type<StringType>();
        descriptor.Field(x => x.Price).Type<DecimalType>();
        descriptor.Field(x => x.Area).Type<IntType>();
        descriptor.Field(x => x.BuiltArea).Type<IntType>();
        descriptor.Field(x => x.Bedrooms).Type<IntType>();
        descriptor.Field(x => x.Status).Type<StringType>();
        descriptor.Field(x => x.Photos).Type<ListType<StringType>>();
        descriptor.Field(x => x.CreatedAt).Type<DateTimeType>();
        descriptor.Field(x => x.UpdatedAt).Type<DateTimeType>();
    }
}