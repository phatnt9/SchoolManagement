using Newtonsoft.Json.Linq;
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
        public enum CLIENTCMD
        {
            REQUEST_PROFILE=100,
            REQUEST_REG_PERSON_LIST=110,
            REQUEST_CHECKALIVE=120,

        }
        public enum SERVERRESPONSE
        {
            RESP_SUCCESS= 200,
            RESP_PROFILE_SUCCESS = 210,
            RESP_SEND_NEWPROFILE_IMMEDIATELY = 240,
            RESP_REQ_PERSONLIST_IMMEDIATELY = 250,
            RESP_DATAFORMAT_ERROR = 300,
            RESP_PERSONLIST_ERROR = 320,



        }
        int publishdata;
        public event Action<String> MessageCallBack;
        private MainWindowModel mainWindowModel;
        public DeviceItem(MainWindowModel mainWindowModel)
        {
            this.mainWindowModel = mainWindowModel;
        }
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
            try
            {
                JObject stuff = JObject.Parse(standard.data);
                var status = (CLIENTCMD)((int)stuff["status"]);
                switch (status)
                {
                    case CLIENTCMD.REQUEST_PROFILE:
                        List<string> serialList=mainWindowModel.GetListSerialId();
                        //dynamic product=new JOb
                        break;
                    case CLIENTCMD.REQUEST_REG_PERSON_LIST:
                        break;

                }
            }
            catch
            {

            }
        }
    }
}
