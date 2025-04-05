using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using tempfunctionapp.model;
namespace tempfunctionapp.service
{
    /// <summary>
    /// Service responsible for updating an Excel file stored in Azure Blob Storage 
    /// with the latest warrior data. It compares the provided list of warriors with 
    /// the existing records in the Excel file and adds or updates rows accordingly. 
    /// </summary>
    public class ExcelService : IExcelService
    {
        private readonly BlobContainerClient _container;
        private readonly ILogger<ExcelService> _logger;
        private const string FileName = "warriors.xlsx";

        public ExcelService(IConfiguration config, ILogger<ExcelService> logger)
        {
            var blobConnection = config["BlobStorageConnectionString"];
            var containerName = config["BlobContainerName"];
            _container = new BlobContainerClient(blobConnection, containerName);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _logger = logger;
        }
        /// <summary>
        /// Updates the warriors.xlsx file in Blob Storage by syncing it with the provided 
        /// list of warriors. Adds new warriors and updates existing ones if the LastUpdated 
        /// timestamp indicates newer data. Trace logs are emitted for each change.
        /// </summary>
        /// <param name="dbWarriors">List of warrior records to sync to the Excel file.</param>
        public async Task UpdateExcelAsync(List<Warrior> dbWarriors)
        {
            await _container.CreateIfNotExistsAsync();
            var blob = _container.GetBlobClient(FileName);
            var stream = new MemoryStream();

            Dictionary<int, Warrior> dbMap = dbWarriors.ToDictionary(w => w.Id);

            if (await blob.ExistsAsync())
            {
                await blob.DownloadToAsync(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);
                var ws = package.Workbook.Worksheets.FirstOrDefault() ?? package.Workbook.Worksheets.Add("Warriors");

                var existingRows = ws.Dimension?.Rows ?? 1;
                var excelMap = new Dictionary<int, int>(); // Id -> row

                for (int row = 2; row <= existingRows; row++)
                {
                    var idText = ws.Cells[row, 1].Text;
                    if (int.TryParse(idText, out int id))
                    {
                        excelMap[id] = row;
                    }
                }

                foreach (var warrior in dbWarriors)
                {
                    if (excelMap.TryGetValue(warrior.Id, out int row))
                    {
                        var lastUpdatedCell = DateTime.Parse(ws.Cells[row, 6].Text);
                        if (warrior.LastUpdated > lastUpdatedCell)
                        {
                            WriteWarriorToRow(ws, warrior, row);
                            _logger.LogTrace($"Updated warrior: {warrior.Name} (Id: {warrior.Id}) at row {row}. DB LastUpdated: {warrior.LastUpdated:o}, Excel LastUpdated: {lastUpdatedCell:o}");
                        }
                    }
                    else
                    {
                        int newRow = ++existingRows;
                        WriteWarriorToRow(ws, warrior, newRow);
                        _logger.LogTrace($"Added new warrior: {warrior.Name} (Id: {warrior.Id}) at row {newRow}.");
                    }
                }

                stream.SetLength(0);
                package.SaveAs(stream);
            }
            else
            {
                using var package = new ExcelPackage();
                var ws = package.Workbook.Worksheets.Add("Warriors");

                ws.Cells[1, 1].Value = "Id";
                ws.Cells[1, 2].Value = "Name";
                ws.Cells[1, 3].Value = "Clan";
                ws.Cells[1, 4].Value = "Strength";
                ws.Cells[1, 5].Value = "Rank";
                ws.Cells[1, 6].Value = "LastUpdated";

                for (int i = 0; i < dbWarriors.Count; i++)
                {
                    WriteWarriorToRow(ws, dbWarriors[i], i + 2);
                    _logger.LogTrace($"Initial write: {dbWarriors[i].Name} (Id: {dbWarriors[i].Id}) at row {i + 2}.");
                }

                package.SaveAs(stream);
            }

            stream.Position = 0;
            await blob.UploadAsync(stream, overwrite: true);
        }
        /// <summary>
        /// Writes a warrior's data to a specific row in the Excel worksheet.
        /// </summary>
        /// <param name="ws">The Excel worksheet to write to.</param>
        /// <param name="w">The warrior whose data will be written.</param>
        /// <param name="row">The target row number in the worksheet.</param>
        private void WriteWarriorToRow(ExcelWorksheet ws, Warrior w, int row)
        {
            ws.Cells[row, 1].Value = w.Id;
            ws.Cells[row, 2].Value = w.Name;
            ws.Cells[row, 3].Value = w.Clan;
            ws.Cells[row, 4].Value = w.Strength;
            ws.Cells[row, 5].Value = w.Rank;
            ws.Cells[row, 6].Value = w.LastUpdated.ToString("o");
        }
    }
}
