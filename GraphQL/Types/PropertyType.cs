using HotChocolate.Types;
using PropertiesService.Models;

namespace PropertiesService.GraphQL.Types
{
    public class PropertyType : ObjectType<Property>
    {
        protected override void Configure(IObjectTypeDescriptor<Property> descriptor)
        {
            descriptor.Name("Property");

            descriptor
                .Field(x => x.IdProperty)
                .Type<NonNullType<UuidType>>();
            
            descriptor
                .Field(x => x.IdUser)
                .Type<NonNullType<UuidType>>();
            
            descriptor
                .Field(x => x.Title)
                .Type<NonNullType<StringType>>();
            
            descriptor
                .Field(x => x.Description)
                .Type<NonNullType<StringType>>();
            
            descriptor
                .Field(x => x.Address)
                .Type<NonNullType<StringType>>();
            
            descriptor
                .Field(x => x.City)
                .Type<NonNullType<StringType>>();
            
            descriptor
                .Field(x => x.Country)
                .Type<NonNullType<StringType>>();
            
            descriptor
                .Field(x => x.PropertyType)
                .Type<NonNullType<StringType>>();
            
            descriptor
                .Field(x => x.TransactionType)
                .Type<NonNullType<StringType>>();
            
            // ¡Usa DecimalType aquí para tu decimal Price!
            descriptor
                .Field(x => x.Price)
                .Type<NonNullType<DecimalType>>();
            
            descriptor
                .Field(x => x.Area)
                .Type<NonNullType<IntType>>();
            
            descriptor
                .Field(x => x.BuiltArea)
                .Type<NonNullType<IntType>>();
            
            descriptor
                .Field(x => x.Bedrooms)
                .Type<NonNullType<IntType>>();
            
            descriptor
                .Field(x => x.Status)
                .Type<NonNullType<StringType>>();
            
            descriptor
                .Field(x => x.Photos)
                .Type<NonNullType<ListType<StringType>>>();
            
            descriptor
                .Field(x => x.CreatedAt)
                .Type<NonNullType<DateTimeType>>();
            
            descriptor
                .Field(x => x.UpdatedAt)
                .Type<NonNullType<DateTimeType>>();
        }
    }
}