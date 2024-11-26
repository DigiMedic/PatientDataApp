using HotChocolate.Types;
using PatientDataApp.Models;
using PatientDataApp.Data;

namespace PatientDataApp.GraphQL.Types;

public class MriImageType : ObjectType<MriImage>
{
    protected override void Configure(IObjectTypeDescriptor<MriImage> descriptor)
    {
        descriptor.Field(m => m.Id).Type<NonNullType<IdType>>();
        descriptor.Field(m => m.PatientId).Type<NonNullType<IntType>>();
        descriptor.Field(m => m.AcquisitionDate).Type<NonNullType<DateTimeType>>();
        descriptor.Field(m => m.ImagePath).Type<NonNullType<StringType>>();
        descriptor.Field(m => m.Description).Type<StringType>();
        descriptor.Field(m => m.Findings).Type<StringType>();
        descriptor.Field(m => m.CreatedAt).Type<NonNullType<DateTimeType>>();
        
        descriptor.Field(m => m.Patient)
            .Type<PatientType>()
            .ResolveWith<Resolvers>(r => r.GetPatientForMriImage(default!, default!))
            .UseDbContext<PatientDbContext>();

        // Přidání URL pro přístup k obrázku
        descriptor.Field("imageUrl")
            .Type<StringType>()
            .Resolve(context =>
            {
                var mriImage = context.Parent<MriImage>();
                return $"/api/file/mri/{mriImage.Id}";
            });
    }
}
