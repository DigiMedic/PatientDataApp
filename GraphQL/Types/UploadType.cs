using HotChocolate.Types;

namespace PatientDataApp.GraphQL.Types
{
    public class UploadType : ScalarType<IFile, string>
    {
        public UploadType() : base("Upload")
        {
            Description = "The `Upload` scalar type represents a file upload.";
        }

        protected override IFile ParseLiteral(IValueNode valueSyntax)
            => throw new NotSupportedException();

        protected override IValueNode ParseValue(IFile value)
            => throw new NotSupportedException();

        public override IValueNode ParseResult(object? resultValue)
            => throw new NotSupportedException();

        protected override string SerializeResult(object? resultValue)
            => throw new NotSupportedException();

        protected override IFile ParseValue(string serializedValue)
            => throw new NotSupportedException();
    }
} 