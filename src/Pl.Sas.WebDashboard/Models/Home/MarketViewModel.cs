namespace Pl.Sas.WebDashboard.Models
{
    public class MarketViewModel
    {
        public List<string> Exchanges { get; set; } = new List<string>();
        public Dictionary<string, string> IndustryCodes { get; set; } = new Dictionary<string, string>();
        public bool UserHasFollowStock { get; set; }
    }
}