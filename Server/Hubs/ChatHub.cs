using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Server.Hubs
{
    public class ChatHub : Hub
    {
        /// <summary>
        /// ارسال پیام به همهٔ کلاینت‌ها
        /// </summary>
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        /// <summary>
        /// ملحق شدن به یک گروه
        /// </summary>
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName)
                         .SendAsync("GroupNotification", $"{Context.ConnectionId} به گروه '{groupName}' ملحق شد.");
        }

        /// <summary>
        /// خروج از یک گروه
        /// </summary>
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName)
                         .SendAsync("GroupNotification", $"{Context.ConnectionId} از گروه '{groupName}' خارج شد.");
        }

        /// <summary>
        /// ارسال پیام به اعضای یک گروه
        /// </summary>
        public async Task SendMessageToGroup(string groupName, string user, string message)
        {
            await Clients.Group(groupName)
                         .SendAsync("ReceiveGroupMessage", user, message);
        }
    }
}
