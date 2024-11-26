using HotChocolate;
using Microsoft.Extensions.Logging;

namespace PatientDataApp.GraphQL;

public class GraphQLErrorFilter : IErrorFilter
{
    private readonly ILogger<GraphQLErrorFilter> _logger;
    private readonly IWebHostEnvironment _env;

    public GraphQLErrorFilter(ILogger<GraphQLErrorFilter> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public IError OnError(IError error)
    {
        var exception = error.Exception;
        if (exception != null)
        {
            _logger.LogError(
                exception,
                "GraphQL Error: {Message}, Path: {Path}, Code: {Code}",
                error.Message,
                error.Path?.Print(),
                error.Code);
        }
        else
        {
            _logger.LogError(
                "GraphQL Error without exception: {Message}, Path: {Path}, Code: {Code}",
                error.Message,
                error.Path?.Print(),
                error.Code);
        }

        if (_env.IsDevelopment())
        {
            return error.WithMessage(error.Exception?.ToString() ?? error.Message);
        }

        return error.WithMessage("An internal error occurred. Please try again later.");
    }
}
