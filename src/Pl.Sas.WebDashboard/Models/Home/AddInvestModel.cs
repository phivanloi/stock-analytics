namespace Pl.Sas.WebDashboard.Models
{
    public class AddInvestModel
    {
        public string Code { get; set; }
        public int Amount { get; set; }
        public decimal Price { get; set; }
        public decimal BuyTax { get; set; }
    }
}