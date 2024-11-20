using HotChocolate.Types;
using PatientDataApp.Models;

namespace PatientDataApp.GraphQL.Types
{
    public class PatientType : ObjectType<Patient>
    {
        protected override void Configure(IObjectTypeDescriptor<Patient> descriptor)
        {
            descriptor.Field(p => p.Id).Type<NonNullType<IdType>>();
            descriptor.Field(p => p.FirstName).Type<NonNullType<StringType>>();
            descriptor.Field(p => p.LastName).Type<NonNullType<StringType>>();
            
            // Přidáme resolver pro MRI snímky pacienta
            descriptor
                .Field("mriImages")
                .ResolveWith<Resolvers>(r => r.GetMriImages(default!, default!))
                .UseDbContext<PatientDbContext>();
        }
    }
} 