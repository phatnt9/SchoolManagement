﻿using SchoolManagement.Model;

namespace SchoolManagement.DTO
{
    public class DeviceRF : NotifyUIBase
    {
        private int _id;
        private string _ip;
        private string _accountClass;
        private string _gate;
        private string _status;
        private string _uploadPercent;
        private DeviceItem _deviceItem;
        private string _connectStatus;

        public int ID { get => _id; set => _id = value; }
        public string IP { get => _ip; set => _ip = value; }
        public string GATE { get => _gate; set => _gate = value; }
        public string CLASS { get => _accountClass; set => _accountClass = value; }
        public string STATUS { get => _status; set => _status = value; }
        public string connectStatus { get => _connectStatus; set { _connectStatus = value; RaisePropertyChanged("connectStatus"); } }
        public string uploadPercent { get => _uploadPercent; set { _uploadPercent = value; RaisePropertyChanged("uploadPercent"); } }
        public DeviceItem deviceItem { get => _deviceItem; set { _deviceItem = value; RaisePropertyChanged("deviceItem"); } }

        public System.Timers.Timer checkAliveDevice;

        public DeviceRF()
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