using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SchoolManagement.Communication;
using SchoolManagement.DTO;
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
        public class JStringProfile
        {
            public int status;
            public List<String> data;
        }
        public class JStringClient
        {
            public int deviceId;
            public int status;
            public List<String> data;
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
            publishdata = this.Advertise("ServerPublish", "std_msgs/String");
            int subscription = this.Subscribe("ClientPublish", "std_msgs/String", DataHandler);
        }
        public void sendProfile()
        {
            JStringProfile Jprofile = new JStringProfile();
            Jprofile.status = (int)SERVERRESPONSE.RESP_PROFILE_SUCCESS;
            Jprofile.data = mainWindowModel.GetListSerialId();
            String dataResp = JsonConvert.SerializeObject(Jprofile).ToString();
            StandardString info = new StandardString();
            info.data = dataResp;
            this.Publish(publishdata, info);
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
                        sendProfile();
                        //dynamic product=new JOb
                        break;
                    case CLIENTCMD.REQUEST_REG_PERSON_LIST:
                        List<Person> personList = new List<Person>();
                        JObject dataClient = JObject.Parse(standard.data);
                        JArray results = new JArray(dataClient["data"]);
                        foreach( var result in results)
                        {
                            String serialId = (String)result["serialId"];
                            String tick = (String)result["tick"];
                            Person person = new Person() { serialId=serialId,tick=tick};
                            personList.Add(person);
                        }
                        if(personList.Count>0)
                            mainWindowModel.CheckinServer(personList);
                        break;

                }
            }
            catch
            {

            }
        }
    }
}
