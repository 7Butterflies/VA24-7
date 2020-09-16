using System;

namespace VA24_7_Shared.Model
{
    public class Activity : IoT
    {
        public bool IsRunning { get; set; } = false;
        public bool IsSleeping { get; set; } = false;
        public int Pulserate { get; set; } = 50;
        public PulseStatus PulseStatus { get; set; } = PulseStatus.Normal;
    }

    public class CloudToDevice : IoT
    {
        public string Comments { get; set; }
        public string ResponseStatus { get; set; }
    }
}
