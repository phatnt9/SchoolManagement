using SchoolManagement.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SchoolManagement.Model
{
   public class DeviceItem:RosSocket
    {
        int publishdata;
        public event Action<String> MessageCallBack;
        public DeviceItem()
        {}
        protected override void OnOpenedEvent()
        {
            try
            {
                createRosTerms();
            }
            catch
            {
                Console.WriteLine("Robot Control Error Send OnOpenedEvent");
            }
        }

        protected override void OnClosedEvent(object sender, CloseEventArgs e)
        {
            base.OnClosedEvent(sender, e);

        }
        public void createRosTerms()
        {
            publishdata = this.Advertise("/testpub", "std_msgs/String");
            int subscription = this.Subscribe("/testsub", "std_msgs/String", DataHandler);
        }
        private void DataHandler(Message message)
        {
            StandardString standard = (StandardString)message;
        }
    }
}
