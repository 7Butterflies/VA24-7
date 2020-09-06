using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VA24_7_Shared.Model;

namespace VA24_7_BandAgent.Hub
{
    public class IoTHub : IIoTHub
    {
        public IoTHub(DeviceClient IoTDeviceClient)
        {
            deviceClient = IoTDeviceClient;

            IoTDeviceClient.SetMethodDefaultHandlerAsync(DefaultCloudToDeviceMessageHandler, null);
            IoTDeviceClient.SetMethodHandlerAsync("CloudToDeviceMessageHandler", CloudToDeviceMessageHandler, null);
        }

        public DeviceClient deviceClient { get; set; }
        public async Task SendDeviceToCloudMessage(Activity activity)
        {
            var payload = JsonConvert.SerializeObject(activity);

            var message = new Message(Encoding.ASCII.GetBytes(payload));

            await deviceClient.SendEventAsync(message);
        }

        private Task<MethodResponse> CloudToDeviceMessageHandler(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine("***MESSAGE RECIEVED***");
            Console.WriteLine(methodRequest.DataAsJson);

            var responsePayload = Encoding.ASCII.GetBytes($"Response is sucessfully recieved: {methodRequest.DataAsJson}");

            return Task.FromResult(new MethodResponse(responsePayload, 200));
        }

        public Task<MethodResponse> DefaultCloudToDeviceMessageHandler(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine("***MESSAGE IS  NOT CALLED***");
            Console.WriteLine("Methid name: :" + methodRequest.Name);
            Console.WriteLine(methodRequest.DataAsJson);

            var responsePayload = Encoding.ASCII.GetBytes($"This method not implemented {methodRequest.DataAsJson}");

            return Task.FromResult(new MethodResponse(responsePayload, 400));
        }


        public async Task UpdateTwin()
        {
            var twinProperties = new TwinCollection();
            twinProperties["connectionType"] = "wi-fi";
            twinProperties["connectionStrength"] = "full";

            await deviceClient.UpdateReportedPropertiesAsync(twinProperties);
        }
    }
}
