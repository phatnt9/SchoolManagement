using SchoolManagement.Model;
using SchoolManagement.ViewModel;

namespace SchoolManagement.Model
{
    public class Device : ViewModelBase
    {
        public int ID { get; set; }
        public string IP { get; set; }
        public string GATE { get; set; }
        public string CLASS { get; set; }
        private string _status;
        public string STATUS { get => _status; set { _status = value; RaisePropertyChanged("STATUS"); } }
        private string _connectStatus;
        public string connectStatus { get => _connectStatus; set { _connectStatus = value; RaisePropertyChanged("connectStatus"); } }

        private string _requestImgPercent;
        public string requestImgPercent { get => _requestImgPercent; set { _requestImgPercent = value; RaisePropertyChanged("requestImgPercent"); } }



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
                else
                {
                    connectStatus = "Disconnected";
                    return;
                }
            }
            connectStatus = "Unknow";
        }
    }
}