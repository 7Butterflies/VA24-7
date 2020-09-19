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

    public class IoTDevice
    {
        public string deviceId { get; set; }
        public string version { get; set; }
        public string status { get; set; }
        public string associationStatus { get; set; }
        public string connectionState { get; set; }
        public string lastActivityTime { get; set; }
        public string cloudToDeviceMessageCount { get; set; }
    }

    public enum IoTMessageType
    {
        deviceToCloud,
        cloudToDevice,
    }
}
