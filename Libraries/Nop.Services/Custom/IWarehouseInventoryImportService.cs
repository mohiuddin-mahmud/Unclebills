using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Custom
{
    public partial interface IWarehouseInventoryImportService
    {
        /// <summary>
        /// Update warehouse inventory counts from csv file
        /// </summary>
        /// <param name="csvFileFullLocalPath">The full path to the csv file on the server</param>
        /// <returns>Message relating to the success of processing the csv file</returns>
        Task<string> Import(string csvFileFullLocalPath);
    }
}
