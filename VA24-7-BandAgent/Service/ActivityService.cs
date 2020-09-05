using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using VA24_7_Shared.Model;

namespace VA24_7_BandAgent.Service
{
    public class ActivityService
    {
        public IConfiguration Configuration { get; set; }
        public ActivityService(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }
        public async Task SendMessageToIoTHub(Activity activity)
        {
            Console.WriteLine("Initializing Band Agent...");

            var device = DeviceClient.CreateFromConnectionString(this.Configuration["DeviceConnectionString"]);

            await device.OpenAsync();

            Console.WriteLine("Device is connected!");

            await UpdateTwin(device);

            RecieveEventsAsync(device);

            await device.SetMethodDefaultHandlerAsync(defaultMessageHandler, null);
            await device.SetMethodHandlerAsync("showMessage", ShowMessage, null);

            var random = new Random();
            var quitRequested = false;

            var payload = JsonConvert.SerializeObject(activity);

            var message = new Message(Encoding.ASCII.GetBytes(payload));

            await device.SendEventAsync(message);

            Console.WriteLine("Message sent!");

            Console.WriteLine("Disconnecting...");
        }

        private static async Task UpdateTwin(DeviceClient device)
        {
            var twinProperties = new TwinCollection();
            twinProperties["connectionType"] = "wi-fi";
            twinProperties["connectionStrength"] = "full";

            await device.UpdateReportedPropertiesAsync(twinProperties);
        }

        //This function will continously run in the background to reciece messages from the cloud.
        private static async Task RecieveEventsAsync(DeviceClient device)
        {
            while (true)
            {
                //It will return as soon as a new message to process or after a default timeout peroid is expired.
                var message = await device.ReceiveAsync();

                if (message == null)
                    continue;
                var messageBody = message.GetBytes();
                var payload = Encoding.ASCII.GetString(messageBody);

                Console.WriteLine("Message recieved from cloud:" + payload);

                await device.CompleteAsync(message);
            }
        }

        private static Task<MethodResponse> ShowMessage(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine("***MESSAGE RECIEVED***");
            Console.WriteLine(methodRequest.DataAsJson);

            var responsePayload = Encoding.ASCII.GetBytes($"Response is sucessfully recieved: {methodRequest.DataAsJson}");

            return Task.FromResult(new MethodResponse(responsePayload, 200));
        }

        private static Task<MethodResponse> defaultMessageHandler(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine("***MESSAGE IS  NOT CALLED***");
            Console.WriteLine("Methid name: :" + methodRequest.Name);
            Console.WriteLine(methodRequest.DataAsJson);

            var responsePayload = Encoding.ASCII.GetBytes($"This method not implemented {methodRequest.DataAsJson}");

            return Task.FromResult(new MethodResponse(responsePayload, 400));
        }

    }
}
