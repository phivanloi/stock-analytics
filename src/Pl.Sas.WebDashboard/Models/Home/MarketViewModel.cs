using System.Collections.Generic;

namespace Pl.Sas.WebDashboard.Models
{
    public class MarketViewModel
    {
        public List<string> Exchanges { get; set; }
        public Dictionary<string, string> IndustryCodes { get; set; }
        public bool UserHasFollowStock { get; set; }
    }
}