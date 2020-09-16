using System;
using System.Collections.Generic;
using System.Text;

namespace VA24_7_Shared.Model
{
    public class IoT
    {
        public string DeviceId { get; set; }
        public IoTMessageType IoTMessageType { get; set; }
        public dynamic SystemProperties { get; set; }
    }

    public enum IoTMessageType
    {
        deviceToCloud,
        cloudToDevice,
    }
}
