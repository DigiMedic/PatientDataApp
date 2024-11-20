using HotChocolate.Types;

namespace PatientDataApp.GraphQL.Types
{
    public class MriImageFilterInputType : InputObjectType
    {
        protected override void Configure(IInputObjectTypeDescriptor descriptor)
        {
            descriptor.Field("patientId").Type<IntType>();
            descriptor.Field("studyType").Type<StringType>();
            descriptor.Field("bodyPart").Type<StringType>();
            descriptor.Field("startDate").Type<DateTimeType>();
            descriptor.Field("endDate").Type<DateTimeType>();
            descriptor.Field("tags").Type<ListType<StringType>>();
        }
    }
} 