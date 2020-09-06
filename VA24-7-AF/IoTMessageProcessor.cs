using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Linq;
using System.Net.Http;
using System.Text;

using VA24_7_Shared.Model;

namespace VA24_7_AF
{
    public static class IoTMessageProcessor
    {
        private static HttpClient client = new HttpClient();

        [FunctionName("GetDeviceToCloudMessages")]
        public static void GetDeviceToCloudMessages([EventHubTrigger("af-process-messages", Connection = "IoTEventHubCompatibleEndpoint")]EventData[] messages,
            [CosmosDB(databaseName: "va24-7-db",collectionName: "deviceToCloudMessages",
            ConnectionStringSetting = "CosmosDBConnection")]
                IAsyncCollector<Activity> asyncActivities,
             ILogger log)
        {
            foreach (var message in messages)
            {
                log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");

                var messageBody = Encoding.UTF8.GetString(message.Body.Array);
                var activity = JsonConvert.DeserializeObject<Activity>(messageBody);

                var deviceId = message.SystemProperties.Where(x => x.Key == "iothub-connection-device-id").Select(y => y.Value).FirstOrDefault()?.ToString();
                activity.DeviceId = deviceId;

                activity.SystemProperties = JObject.FromObject(message.SystemProperties);

                asyncActivities.AddAsync(activity);
            }
        }
    }
}