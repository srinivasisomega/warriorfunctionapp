using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tempfunctionapp.model;

namespace tempfunctionapp.service
{
    /// <summary>
    /// Service responsible for updating an Excel file stored in Azure Blob Storage 
    /// with the latest warrior data. It compares the provided list of warriors with 
    /// the existing records in the Excel file and adds or updates rows accordingly. 
    /// </summary>
    public interface IExcelService
    {
        /// <summary>
        /// Updates the warriors.xlsx file in Blob Storage by syncing it with the provided 
        /// list of warriors. Adds new warriors and updates existing ones if the LastUpdated 
        /// timestamp indicates newer data. Trace logs are emitted for each change.
        /// </summary>
        /// <param name="dbWarriors">List of warrior records to sync to the Excel file.</param>
        Task UpdateExcelAsync(List<Warrior> latestData);
    }
}
