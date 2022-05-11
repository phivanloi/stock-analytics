using Pl.Sas.Core.Entities;
using System.Collections.Generic;

namespace Pl.Sas.WebDashboard.Models
{
    public class InvestListViewModel
    {
        public List<string> UserFollowSymbols { get; set; }
        public List<StockView> StockViews { get; set; }
    }
}