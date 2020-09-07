using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VA24_7_Shared.Model;

namespace VA24_7_BandAgent.Hub
{
    public interface IIoTHub
    {
        Task SendDeviceToCloudMessage(Activity activity);
        Task UpdateTwin();
    }
}
