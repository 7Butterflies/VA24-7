using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using VA24_7_BandAgent.Hub;
using VA24_7_Shared.Model;

namespace VA24_7_BandAgent.Service
{
    public class ActivityService
    {
        public IConfiguration Configuration { get; set; }
        public IIoTHub IoTHub { get; set; }
        public ActivityService(IConfiguration configuration, IIoTHub ioTHub)
        {
            this.Configuration = configuration;
            this.IoTHub = ioTHub;
        }

        public async Task sendAsync(Activity activity)
        {
            await this.IoTHub.SendDeviceToCloudMessage(activity);
        }

    }
}
