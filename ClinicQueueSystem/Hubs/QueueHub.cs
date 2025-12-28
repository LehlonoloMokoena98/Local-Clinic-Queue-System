using Microsoft.AspNetCore.SignalR;

namespace ClinicQueueSystem.Hubs
{
    public class QueueHub : Hub
    {
        public async Task NotifyQueueUpdate()
        {
            await Clients.All.SendAsync("QueueUpdated");
        }
    }
}
