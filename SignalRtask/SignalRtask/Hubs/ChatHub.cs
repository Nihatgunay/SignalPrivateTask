using Microsoft.AspNetCore.SignalR;

namespace SignalRtask.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessageAsync(string username, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", username, message);
        }

	}
}
