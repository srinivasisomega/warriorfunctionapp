using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tempfunctionapp.model;

namespace tempfunctionapp.service
{
    public interface IExcelService
    {
        Task UpdateExcelAsync(List<Warrior> latestData);
    }
}
