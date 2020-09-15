using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace VA24_7_AF
{
    public class AF_ChatHub : ServerlessHub
    {
        private const string NewConnectionTarget = "newConnection";
        private const string NewMessageTarget = "newMessage";

        [FunctionName(nameof(GetConversations))]
        public static dynamic GetConversations(
     [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "conversations/{id}/{partitionkey}")]HttpRequestMessage req, string id, string partitionkey,
     [CosmosDB(
                databaseName: "VA24-7-DB",
                collectionName: "Chat",
                ConnectionStringSetting = "CosmosDBConnection",SqlQuery = "select * from c where c.id = {id}", PartitionKey ="{partitionkey}")] IEnumerable<dynamic> items,
      ILogger log)
        {
            return items;
        }

        [FunctionName(nameof(SendMessage))]
        public static Task SendMessage(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sendMessage/{senderPersonId}/{recipientPersonId}")]HttpRequestMessage req, string senderPersonId, string recipientPersonId,
      [CosmosDB(
                databaseName: "VA24-7-DB",
                collectionName: "Chat",
                ConnectionStringSetting = "CosmosDBConnection")] DocumentClient documentClient,
      [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages,
       ILogger log)
        {
            dynamic str = Task.Run(async () => { return await req.Content.ReadAsStringAsync(); }).Result;
            var message = JsonConvert.DeserializeObject<dynamic>(str as string);

            try
            {
                message.messageId = Guid.NewGuid();

                var personIds = new[] { senderPersonId, recipientPersonId };
                Array.Sort(personIds);

                var documentId = $"document.conversation-{String.Join("-", personIds)}";
                var partitionKey = $"partition.conversation-{String.Join("-", personIds)}";

                Uri collectionUri = UriFactory.CreateDocumentCollectionUri("VA24-7-DB", "Chat");
                var feedOptions = new FeedOptions { EnableCrossPartitionQuery = true, PartitionKey = new PartitionKey(partitionKey) };

                var queryableDocument = documentClient.CreateDocumentQuery(collectionUri, feedOptions)
                    .Where(t => t.Id == documentId)
                    .AsEnumerable()
                    .FirstOrDefault();

                JObject conversation;
                if (queryableDocument == null)
                {
                    conversation = new JObject
                    {
                        ["messages"] = new JArray(),
                        ["partitionKey"] = partitionKey,
                        ["id"] = documentId
                    };
                }
                else
                {
                    conversation = JObject.Parse(queryableDocument.ToString());
                }

             ((JArray)conversation["messages"]).Add(message);
                var requestOptions = new RequestOptions() { PartitionKey = new PartitionKey(partitionKey) };

                documentClient.UpsertDocumentAsync(collectionUri, conversation, requestOptions);
            }
            catch (Exception ex)
            {
                log.LogError("Failed to add message to document db", ex.Message);
            }

            try
            {
                return signalRMessages.AddAsync(
                                  new SignalRMessage
                                  {
                                      UserId = recipientPersonId,
                                      Target = "newMessage",
                                      Arguments = new[] { message }
                                  });
            }
            catch (Exception ex)
            {
                log.LogError("Failed to send message to the signalR client", ex.Message);

                //Flush the session as we dont have any message to send.
                return signalRMessages.FlushAsync();
            }
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
    }
}