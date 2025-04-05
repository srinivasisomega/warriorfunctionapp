using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tempfunctionapp.model
{
    public class Warrior
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Clan { get; set; }
        public int Strength { get; set; }
        public string Rank { get; set; }
        public DateTime LastUpdated { get; set; }
    }

}
