using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using tempfunctionapp.model;
namespace tempfunctionapp.service
{

    public class ExcelService : IExcelService
    {
        private readonly BlobContainerClient _container;
        private const string FileName = "warriors.xlsx";

        public ExcelService(IConfiguration config)
        {
            var blobConnection = config["BlobStorageConnectionString"];
            var containerName = config["BlobContainerName"];
            _container = new BlobContainerClient(blobConnection, containerName);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        }

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

                var headerRow = 1;
                var existingRows = ws.Dimension?.Rows ?? 1;
                var excelMap = new Dictionary<int, int>(); // Id -> row

                for (int row = 2; row <= existingRows; row++)
                {
                    var id = int.Parse(ws.Cells[row, 1].Text);
                    excelMap[id] = row;
                }

                foreach (var warrior in dbWarriors)
                {
                    if (excelMap.TryGetValue(warrior.Id, out int row))
                    {
                        var lastUpdatedCell = DateTime.Parse(ws.Cells[row, 6].Text);
                        if (warrior.LastUpdated > lastUpdatedCell)
                        {
                            WriteWarriorToRow(ws, warrior, row);
                        }
                    }
                    else
                    {
                        int newRow = ++existingRows;
                        WriteWarriorToRow(ws, warrior, newRow);
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
                    WriteWarriorToRow(ws, dbWarriors[i], i + 2);

                package.SaveAs(stream);
            }

            stream.Position = 0;
            await blob.UploadAsync(stream, overwrite: true);
        }

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
