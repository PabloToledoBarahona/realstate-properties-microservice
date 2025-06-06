using HotChocolate.Types;
using PropertiesService.Dtos;

namespace PropertiesService.GraphQL.Types
{
    public class UpdatePropertyInputType : InputObjectType<UpdatePropertyInput>
    {
        protected override void Configure(IInputObjectTypeDescriptor<UpdatePropertyInput> descriptor)
        {
            descriptor.Name("UpdatePropertyInput");

            descriptor.Field(f => f.Title).Type<StringType>();
            descriptor.Field(f => f.Description).Type<StringType>();
            descriptor.Field(f => f.Address).Type<StringType>();
            descriptor.Field(f => f.City).Type<StringType>();
            descriptor.Field(f => f.Country).Type<StringType>();
            descriptor.Field(f => f.PropertyType).Type<StringType>();
            descriptor.Field(f => f.TransactionType).Type<StringType>();
            
            descriptor.Field(f => f.Price).Type<DecimalType>();
            
            descriptor.Field(f => f.Area).Type<IntType>();
            descriptor.Field(f => f.BuiltArea).Type<IntType>();
            descriptor.Field(f => f.Bedrooms).Type<IntType>();
            descriptor.Field(f => f.Status).Type<StringType>();
            descriptor.Field(f => f.Photos).Type<ListType<StringType>>();
        }
    }
}