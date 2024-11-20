using HotChocolate;
using Microsoft.Extensions.Logging;

namespace PatientDataApp.GraphQL;

public class GraphQLErrorFilter : IErrorFilter
{
    private readonly ILogger<GraphQLErrorFilter> _logger;

    public GraphQLErrorFilter(ILogger<GraphQLErrorFilter> logger)
    {
        _logger = logger;
    }

    public IError OnError(IError error)
    {
        _logger.LogError(error.Exception, "GraphQL Error: {Message}", error.Message);
        
        return error.WithMessage(error.Exception?.Message ?? error.Message);
    }
}
