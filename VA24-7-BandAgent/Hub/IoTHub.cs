using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;

using Newtonsoft.Json;

using System;
using System.Text;
using System.Threading.Tasks;
using VA24_7_BandAgent.Service;
using VA24_7_BandAgent.Shared;

using VA24_7_Shared.Model;

namespace VA24_7_BandAgent.Hub
{
    public class IoTHub : IIoTHub
    {

        public async Task SendDeviceToCloudMessage(Activity activity)
        {
            var payload = JsonConvert.SerializeObject(activity);

            var message = new Message(Encoding.ASCII.GetBytes(payload));

            await IoTService.IoTDeviceClient.SendEventAsync(message);
        }

        public async Task UpdateTwin()
        {
            var twinProperties = new TwinCollection();
            twinProperties["connectionType"] = "wi-fi";
            twinProperties["connectionStrength"] = "full";

            await IoTService.IoTDeviceClient.UpdateReportedPropertiesAsync(twinProperties);
        }

    }
}