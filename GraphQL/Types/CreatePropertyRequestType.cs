using HotChocolate.Types;
using PropertiesService.Dtos;

namespace PropertiesService.GraphQL.Types;

public class CreatePropertyRequestType : InputObjectType<CreatePropertyRequest>
{
    protected override void Configure(IInputObjectTypeDescriptor<CreatePropertyRequest> descriptor)
    {
        descriptor.Name("CreatePropertyRequestInput");

        descriptor.Field(f => f.Title).Type<NonNullType<StringType>>();
        descriptor.Field(f => f.Description).Type<NonNullType<StringType>>();
        descriptor.Field(f => f.Address).Type<NonNullType<StringType>>();
        descriptor.Field(f => f.City).Type<NonNullType<StringType>>();
        descriptor.Field(f => f.Country).Type<NonNullType<StringType>>();
        descriptor.Field(f => f.PropertyType).Type<NonNullType<StringType>>();
        descriptor.Field(f => f.TransactionType).Type<NonNullType<StringType>>();
        descriptor.Field(f => f.Price).Type<NonNullType<DecimalType>>();
        descriptor.Field(f => f.Area).Type<NonNullType<IntType>>();
        descriptor.Field(f => f.BuiltArea).Type<NonNullType<IntType>>();
        descriptor.Field(f => f.Bedrooms).Type<NonNullType<IntType>>();
        descriptor.Field(f => f.Photos).Type<ListType<StringType>>();
    }
}