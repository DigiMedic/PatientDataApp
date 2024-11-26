using Microsoft.EntityFrameworkCore;
using PatientDataApp.Data;

namespace PatientDataApp.Commands;

public class GenerateTestDataCommand
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GenerateTestDataCommand> _logger;

    public GenerateTestDataCommand(
        IServiceProvider serviceProvider,
        ILogger<GenerateTestDataCommand> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync(int patientCount = 50)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PatientDbContext>();
            var generator = new TestDataGenerator(
                context,
                scope.ServiceProvider.GetRequiredService<ILogger<TestDataGenerator>>());

            await generator.GenerateTestDataAsync(patientCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při spouštění generátoru testovacích dat");
            throw;
        }
    }
}
