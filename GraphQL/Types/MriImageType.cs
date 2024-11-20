using HotChocolate.Types;
using PatientDataApp.Models;

namespace PatientDataApp.GraphQL.Types
{
    public class MriImageType : ObjectType<MriImage>
    {
        protected override void Configure(IObjectTypeDescriptor<MriImage> descriptor)
        {
            descriptor.Field(m => m.Id).Type<NonNullType<IdType>>();
            descriptor.Field(m => m.PatientId).Type<NonNullType<IntType>>();
            descriptor.Field(m => m.FileName).Type<NonNullType<StringType>>();
            descriptor.Field(m => m.FileFormat).Type<StringType>();
            descriptor.Field(m => m.UploadedAt).Type<NonNullType<DateTimeType>>();
            
            // Skryjeme binární data z GraphQL API
            descriptor.Field(m => m.FileData).Ignore();
            
            // Přidáme resolver pro pacienta
            descriptor
                .Field(m => m.Patient)
                .ResolveWith<Resolvers>(r => r.GetPatient(default!, default!))
                .UseDbContext<PatientDbContext>();

            // Přidáme pole pro URL náhledu
            descriptor
                .Field("previewUrl")
                .Type<StringType>()
                .Resolve(context => 
                    $"/api/mriimage/{context.Parent<MriImage>().Id}/preview");
        }
    }
} 