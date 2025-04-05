using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using tempfunctionapp.repository;
using tempfunctionapp.service;

public class SyncWarriorsFunction
{
    private readonly IWarriorRepository _repo;
    private readonly IExcelService _excel;

    public SyncWarriorsFunction(IWarriorRepository repo, IExcelService excel)
    {
        _repo = repo;
        _excel = excel;
    }

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
