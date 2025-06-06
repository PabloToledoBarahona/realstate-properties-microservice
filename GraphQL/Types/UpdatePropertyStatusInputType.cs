using HotChocolate.Types;
using PropertiesService.Dtos;

namespace PropertiesService.GraphQL.Types
{
    public class UpdatePropertyStatusInputType : InputObjectType<UpdatePropertyStatusInput>
    {
        protected override void Configure(IInputObjectTypeDescriptor<UpdatePropertyStatusInput> descriptor)
        {
            descriptor.Name("UpdatePropertyStatusInput");

            descriptor
                .Field(f => f.Status)
                .Type<NonNullType<StringType>>()
                .Description("Nuevo estado: 'activa', 'reservada', 'vendida' o 'cancelada'.");
        }
    }
}