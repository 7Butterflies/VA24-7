using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Documents;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace VA24_7_AF
{
    public class AF_ChatHub : ServerlessHub
    {
        private const string NewMessageTarget = "newMessage";
        private const string NewConnectionTarget = "newConnection";

        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
             [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
             [SignalRConnectionInfo(HubName = "chat")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName(nameof(SignalRConnection))]
        public static SignalRConnectionInfo SignalRConnection(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "connection/{userId?}")] HttpRequest req, string userId,
           IBinder binder,
           ILogger log)
        {
            var connectionInfo = binder.Bind<SignalRConnectionInfo>(new SignalRConnectionInfoAttribute { HubName = "chat", UserId = userId });
            return connectionInfo;
        }

        [FunctionName("SendMessage")]
        public static Task SendMessage(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sendMessage/{userId?}")]HttpRequestMessage req, string userId,
      [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            dynamic str = Task.Run(async () => { return await req.Content.ReadAsStringAsync(); }).Result;
            var message = JsonConvert.DeserializeObject<JObject>(str as string);

            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    UserId = userId,
                    Target = "newMessage",
                    Arguments = new[] { message }
                });
        }

        [FunctionName("SendToUser")]
        public async Task SendToUser([SignalRTrigger]InvocationContext invocationContext, string userid, string message)
        {
            await Clients.User(userid).SendAsync(NewMessageTarget, new NewMessage(invocationContext, message));
        }

        [FunctionName("SendToAll")]
        public async Task SendToAll([SignalRTrigger]InvocationContext invocationContext, string message)
        {
            await Clients.All.SendAsync(NewMessageTarget, new NewMessage(invocationContext, message));
        }
    }
}
