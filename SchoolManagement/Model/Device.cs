using SchoolManagement.Model;

namespace SchoolManagement.Model
{
    public class Device
    {
        public int ID { get; set; }
        public string IP { get; set; }
        public string GATE { get; set; }
        public string CLASS { get; set; }
        public string STATUS { get; set; }
        public string connectStatus { get; set; }
        public DeviceItem deviceItem { get; set; }

        public System.Timers.Timer checkAliveDevice;

        public Device()
        {
            connectStatus = "Unknow";
            checkAliveDevice = new System.Timers.Timer();
            checkAliveDevice.Interval = 2000;
            checkAliveDevice.Elapsed += CheckAliveDevice_Elapsed;
            checkAliveDevice.AutoReset = true;
            checkAliveDevice.Start();
        }

        private void CheckAliveDevice_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (deviceItem != null)
            {
                if (deviceItem.webSocket != null)
                {
                    connectStatus = deviceItem.webSocket.IsAlive ? "Connected" : "Disconnected";
                    return;
                }
            }
            connectStatus = "Unknow";
        }
    }
}