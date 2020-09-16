using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Devices;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VA24_7_Shared.Model;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;

namespace VA24_7_AF
{
    public static class AF_IoTMessageProcessor
    {
        private static HttpClient client = new HttpClient();

        [FunctionName("AF_GetDeviceToCloudMessages")]
        public static void AF_GetDeviceToCloudMessages([EventHubTrigger("af-process-messages", Connection = "IoTEventHubCompatibleEndpoint")]EventData[] messages,
            [CosmosDB(databaseName: "VA24-7-DB",collectionName: "IoT",
            ConnectionStringSetting = "CosmosDBIoTConnection")]DocumentClient documentClient,
             ILogger log)
        {
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("VA24-7-DB", "IoT");

            foreach (var message in messages)
            {
                log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");

                var messageBody = Encoding.UTF8.GetString(message.Body.Array);
                var activity = JsonConvert.DeserializeObject<Activity>(messageBody);

                var deviceId = message.SystemProperties.Where(x => x.Key == "iothub-connection-device-id").Select(y => y.Value).FirstOrDefault()?.ToString();
                activity.DeviceId = deviceId;
                activity.IoTMessageType = IoTMessageType.deviceToCloud;

                activity.SystemProperties = JObject.FromObject(message.SystemProperties);

                var partitionKey = $"{activity.IoTMessageType}-{activity.DeviceId}";

                JObject document;
                document = new JObject
                {
                    ["activity"] = JObject.FromObject(activity),
                    ["partitionKey"] = partitionKey,
                };

                var requestOptions = new RequestOptions() { PartitionKey = new PartitionKey(partitionKey) };

                documentClient.UpsertDocumentAsync(collectionUri, document, requestOptions);
            }
        }

        [FunctionName("AF_SendCloudToDeviceMessage")]
        public static async Task<HttpResponseMessage> AF_SendCloudToDeviceMesage(
      [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cloudToDevice")] HttpRequestMessage req,
     [CosmosDB(databaseName: "VA24-7-DB",collectionName: "IoT",
            ConnectionStringSetting = "CosmosDBIoTConnection")]
                DocumentClient documentClient,
      ILogger log)
        {
            try
            {
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri("VA24-7-DB", "IoT");

                dynamic jsonString = await req.Content.ReadAsStringAsync();

                var cloudToDeviceMessage = JsonConvert.DeserializeObject<CloudToDevice>(jsonString as string);

                var response = await CallDirectMethod(JsonConvert.SerializeObject(cloudToDeviceMessage), cloudToDeviceMessage.DeviceId);

                cloudToDeviceMessage.IoTMessageType = IoTMessageType.cloudToDevice;
                cloudToDeviceMessage.ResponseStatus = response?.Status.ToString();

                var partitionKey = $"{ cloudToDeviceMessage.IoTMessageType}-{cloudToDeviceMessage.DeviceId}";

                JObject document;
                document = new JObject
                {
                    ["message"] = JObject.FromObject(cloudToDeviceMessage),
                    ["partitionKey"] = partitionKey,
                };

                var requestOptions = new RequestOptions() { PartitionKey = new PartitionKey(partitionKey) };

                await documentClient.UpsertDocumentAsync(collectionUri, document, requestOptions);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("", Encoding.UTF8, "application/json")
                };
            }
            catch (Exception ex)
            {
                var ResponseFail = new HttpResponseMessage(HttpStatusCode.ExpectationFailed);
                ResponseFail.Content = new StringContent(ex.Message);
                return ResponseFail;
            }
        }

        //Direct method does not work for offline devices.
        private static async Task<CloudToDeviceMethodResult> CallDirectMethod(string message, string deviceId)
        {
            var serviceClient = ServiceClient.CreateFromConnectionString(Environment.GetEnvironmentVariable("ServiceConnectionString"));
            //The name of the method to send should match with the method while recieving.
            //If the method name is not found it will trigger the default method handler in the device.
            var method = new CloudToDeviceMethod("CloudToDeviceMessageHandler");
            //This has to be a valid json expression.
            method.SetPayloadJson($"'{message}'");

            var response = await serviceClient.InvokeDeviceMethodAsync(deviceId, method);

            Console.WriteLine($"Response status: {response.Status}, payload: {response.GetPayloadAsJson()}");

            return response;
        }

    }
}