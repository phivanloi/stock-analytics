using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Pl.Sas.WebDashboard.RealtimeHub
{
    public class StockRealtimeHub : Hub
    {
        public async Task SendUpdateStockView()
        {
            await Clients.All.SendAsync("UpdateStockView");
        }
    }
}