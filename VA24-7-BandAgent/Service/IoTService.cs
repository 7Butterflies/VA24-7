using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VA24_7_BandAgent.Shared;
using VA24_7_Shared.Model;

namespace VA24_7_BandAgent.Service
{
    public static class IoTService
    {
        public static DeviceClient IoTDeviceClient { get; set; }
        public static async Task ConnectToDevice(string deviceCS)
        {
            if (IoTDeviceClient != null)
            {
                await IoTDeviceClient.CloseAsync();
            }

            IoTDeviceClient = DeviceClient.CreateFromConnectionString(deviceCS);
            await IoTDeviceClient.OpenAsync();

            IoTDeviceClient.SetMethodDefaultHandlerAsync(DefaultCloudToDeviceMessageHandler, null);
            IoTDeviceClient.SetMethodHandlerAsync("CloudToDeviceMessageHandler", CloudToDeviceMessageHandler, null);
        }


        private static async Task<MethodResponse> CloudToDeviceMessageHandler(MethodRequest methodRequest, object userContext)
        {
            try
            {
                Console.WriteLine("***MESSAGE RECIEVED***");
                Console.WriteLine(methodRequest.DataAsJson);

                var message = JsonConvert.DeserializeObject<CloudToDevice>(JsonConvert.DeserializeObject(Encoding.ASCII.GetString(methodRequest.Data)) as string);

                await MainLayout.JSRuntime.InvokeAsync<object>("ShowSuccessAlert", new object[] { message.Comments });

                var responsePayload = Encoding.ASCII.GetBytes($"Response is sucessfully recieved: {methodRequest.DataAsJson}");

                return await Task.FromResult(new MethodResponse(responsePayload, 200));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(new MethodResponse(Encoding.ASCII.GetBytes("Failed to parse the message"), 417));
            }
        }

        public static Task<MethodResponse> DefaultCloudToDeviceMessageHandler(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine("***MESSAGE IS  NOT CALLED***");
            Console.WriteLine("Methid name: :" + methodRequest.Name);
            Console.WriteLine(methodRequest.DataAsJson);

            var responsePayload = Encoding.ASCII.GetBytes($"This method not implemented {methodRequest.DataAsJson}");

            return Task.FromResult(new MethodResponse(responsePayload, 400));
        }
    }
}
