using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using tempfunctionapp.repository;
using tempfunctionapp.service;
/// <summary>
/// Azure Function that runs on a timer to synchronize warrior data from a repository
/// into an Excel file stored in Azure Blob Storage.
/// </summary>
public class SyncWarriorsFunction
{
    private readonly IWarriorRepository _repo;
    private readonly IExcelService _excel;

    public SyncWarriorsFunction(IWarriorRepository repo, IExcelService excel)
    {
        _repo = repo;
        _excel = excel;
    }
    /// <summary>
    /// Timer-triggered Azure Function that runs every minute to fetch warrior data
    /// from the repository and update the corresponding Excel file in Blob Storage.
    /// </summary>
    /// <param name="timer">Timer trigger information.</param>
    /// <param name="context">Function context for logging and execution environment.</param>

    [Function("SyncWarriors")]
    public async Task RunAsync([TimerTrigger("0 */1 * * * *")] TimerInfo timer, FunctionContext context)
    {
        var logger = context.GetLogger("SyncWarriors");
        logger.LogInformation($"Function started at: {DateTime.UtcNow}");

        var data = await _repo.GetAllWarriorsAsync();
        await _excel.UpdateExcelAsync(data);

        logger.LogInformation("Excel file synced with database.");
    }
}
