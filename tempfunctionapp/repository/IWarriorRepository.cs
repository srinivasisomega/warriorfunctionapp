using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tempfunctionapp.model;

namespace tempfunctionapp.repository
{
    public interface IWarriorRepository
    {
        Task<List<Warrior>> GetAllWarriorsAsync();
    }

}
