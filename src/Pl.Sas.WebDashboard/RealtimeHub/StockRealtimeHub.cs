using Microsoft.AspNetCore.SignalR;

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