using HotChocolate.Types;
using PatientDataApp.Models;
using PatientDataApp.Data;

namespace PatientDataApp.GraphQL.Types;

public class DiagnosticResultType : ObjectType<DiagnosticResult>
{
    protected override void Configure(IObjectTypeDescriptor<DiagnosticResult> descriptor)
    {
        descriptor.Field(d => d.Id).Type<NonNullType<IdType>>();
        descriptor.Field(d => d.PatientId).Type<NonNullType<IntType>>();
        descriptor.Field(d => d.Diagnosis).Type<NonNullType<StringType>>();
        descriptor.Field(d => d.Description).Type<StringType>();
        descriptor.Field(d => d.Date).Type<NonNullType<DateTimeType>>();
        
        descriptor.Field(d => d.Patient)
            .Type<PatientType>();
    }
}
