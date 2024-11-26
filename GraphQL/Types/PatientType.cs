using HotChocolate.Types;
using PatientDataApp.Models;
using PatientDataApp.Data;

namespace PatientDataApp.GraphQL.Types;

public class PatientType : ObjectType<Patient>
{
    protected override void Configure(IObjectTypeDescriptor<Patient> descriptor)
    {
        descriptor.Field(p => p.Id).Type<NonNullType<IdType>>();
        descriptor.Field(p => p.FirstName).Type<NonNullType<StringType>>();
        descriptor.Field(p => p.LastName).Type<NonNullType<StringType>>();
        descriptor.Field(p => p.DateOfBirth).Type<NonNullType<DateTimeType>>();
        descriptor.Field(p => p.PersonalId).Type<NonNullType<StringType>>();
        descriptor.Field(p => p.LastDiagnosis).Type<StringType>();
        descriptor.Field(p => p.InsuranceCompany).Type<StringType>();
        descriptor.Field(p => p.LastExaminationDate).Type<DateTimeType>();
        descriptor.Field(p => p.CreatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(p => p.UpdatedAt).Type<NonNullType<DateTimeType>>();
        
        descriptor.Field(p => p.DiagnosticResults)
            .Type<ListType<DiagnosticResultType>>()
            .ResolveWith<Resolvers>(r => r.GetDiagnosticResults(default!, default!))
            .UseDbContext<PatientDbContext>();
        
        descriptor.Field(p => p.MriImages)
            .Type<ListType<MriImageType>>()
            .ResolveWith<Resolvers>(r => r.GetMriImages(default!, default!))
            .UseDbContext<PatientDbContext>();

        // Přidání pomocných polí
        descriptor.Field("age")
            .Type<IntType>()
            .Resolve(context => context.Parent<Patient>().GetAge());

        descriptor.Field("fullName")
            .Type<StringType>()
            .Resolve(context => context.Parent<Patient>().GetFullName());
    }
}
