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
        
        descriptor.Field(p => p.DiagnosticResults)
            .Type<ListType<DiagnosticResultType>>();
        
        descriptor.Field(p => p.MriImages)
            .Type<ListType<MriImageType>>();
    }
}
