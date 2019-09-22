using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SchoolManagement.Communication;
using SchoolManagement.Model;
using SchoolManagement.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using WebSocketSharp;

namespace SchoolManagement.Model
{
    public class DeviceItem : RosSocket
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public enum STATUSPROFILE
        {
            Ready,
            Updated,
            Updating,
            Failed,
            Error,
            Pending,
        }

        public enum CLIENTCMD
        {
            CLIENT_READY = 1000,
            REQUEST_PROFILE_ADD = 110,
            REQUEST_PROFILE_UPDATE = 111,
            REQUEST_PROFILE_DELETE = 112,
            REQUEST_REG_PERSON_LIST = 120,
            REQUEST_SYNC_TIME = 130,
            CONFIRM_ADD_PROFILE_SUCCESS = 310,
            CONFIRM_UPDATE_PROFILE_SUCCESS = 311,
            CONFIRM_DELETE_PROFILE_SUCCESS = 312,
            CONFIRM_ADD_PROFILE_UNSUCCESS = 320,
            CONFIRM_UPDATE_PROFILE_UNSUCCESS = 321,
            CONFIRM_DELETE_PROFILE_UNSUCCESS = 322,
        }

        public enum SERVERRESPONSE
        {
            RESP_SUCCESS = 200,
            RESP_PROFILE_ADD = 210,
            RESP_PROFILE_UPDATE = 211,
            RESP_PROFILE_DELETE = 212,
            RESP_SYNC_TIME = 220,
            RESP_SEND_NEWPROFILE_IMMEDIATELY = 240,
            RESP_REQ_PERSONLIST_IMMEDIATELY = 250,
            RESP_DATAFORMAT_ERROR = 300,
            RESP_PERSONLIST_ERROR = 320,
        }

        //public enum

        public class JStringProfile
        {
            public int status;
            public List<Profile> data;
        }

        public class JStringClient
        {
            public int deviceId;
            public int status;
            public List<String> data;
        }

        public struct FLAGSTATUSCLIENT
        {
            public CLIENTCMD OnConfirmProfileSuccess;
        }

        private int publishdata;
        private int publishdataImg;

        //public event Action<String> MessageCallBack;
        public FLAGSTATUSCLIENT OnFlagStatusClient;

        private MainWindowModel mainWindowModel;
        public STATUSPROFILE statusProfile = STATUSPROFILE.Pending;

        public DeviceItem(MainWindowModel mainWindowModel)
        {
            this.mainWindowModel = mainWindowModel;

            OnFlagStatusClient.OnConfirmProfileSuccess = CLIENTCMD.CLIENT_READY;
        }

        protected override void OnOpenedEvent()
        {
            try
            {
                createRosTerms();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Robot Control Error Send OnOpenedEvent");
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }

        protected override void OnClosedEvent(object sender, CloseEventArgs e)
        {
            base.OnClosedEvent(sender, e);
        }

        public void ReqImgHandler(Message message)
        {
            StandardString standard = (StandardString)message;
            dynamic stuff = JObject.Parse(standard.data);
            string ip = stuff.ip;
            string name = stuff.img;
            int percent = (int)stuff.percent;

            Constant.mainWindowPointer.WriteLog("["+ip+"]"+"  Image loaded: "+ percent+"%"); //  [192.168.105.5]  Image loaded: 40%

            SensorCompressedImage imgdata = new SensorCompressedImage();

            try
            {
                //  BitmapImage img = new System.Windows.Media.Imaging.BitmapImage(new Uri(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + standard.data));
                String pathImg = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + name;
                if (!File.Exists(pathImg))
                {
                    //string test = @"pack://siteoforigin:,,,/Resources/" + "default.png";
                    //Image img = Image.FromFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + "default.png");
                    //Image img = null;
                    imgdata.format = "NoImage";
                    //imgdata.data = ImageToByte(img);
                    if (webSocket.IsAlive)
                    {
                        this.Publish(publishdataImg, imgdata);
                    }
                }
                else
                {
                    Image img = Image.FromFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + name);
                    Image resizeImg = new Bitmap(img, new Size(300, 400));
                    imgdata.format = name;
                    imgdata.data = ImageToByte(resizeImg);
                    if (webSocket.IsAlive)
                    {
                        this.Publish(publishdataImg, imgdata);
                    }
                }
            }
            catch
            {
                //Image img = Image.FromFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + "default.png");
                //Image img = null;
                imgdata.format = "NoImage";
                //imgdata.data = ImageToByte(img);
                if (webSocket.IsAlive)
                {
                    this.Publish(publishdataImg, imgdata);
                }
            }
        }

        public Byte[] ImageToByte(Image img)
        {
            byte[] byteArray = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Close();

                byteArray = stream.ToArray();
            }
            return byteArray;
        }

        public void createRosTerms()
        {
            int subscription_imagerequest = this.Subscribe("ReqImage", "std_msgs/String", ReqImgHandler);
            publishdataImg = this.Advertise("ServerRespImg", "sensor_msgs/CompressedImage");

            publishdata = this.Advertise("ServerPublish", "std_msgs/String");
            int subscription = this.Subscribe("ClientPublish", "std_msgs/String", DataHandler);
        }

        public void sendProfile(string ip, SERVERRESPONSE serverRes, List<Profile> listProfileToSend)
        {
            if (webSocket != null)
            {
                if (webSocket.IsAlive)
                {
                    try
                    {
                        if (statusProfile != STATUSPROFILE.Updating)
                        {
                            JStringProfile Jprofile = new JStringProfile();
                            Jprofile.status = (int)serverRes;
                            
                            if (listProfileToSend.Count > 0)
                            {
                                Jprofile.data = listProfileToSend;
                            }
                            else
                            {
                                //if (!mainWindowModel.IsModifyDataGridOpen)
                                {
                                    Jprofile.data = mainWindowModel.GetListSerialId(ip);
                                }
                            }
                            var jsonSettings = new JsonSerializerSettings();
                            jsonSettings.DateFormatString = "yyyy-MM-dd";
                            string dataResp = JsonConvert.SerializeObject(Jprofile, jsonSettings);
                            StandardString info = new StandardString();
                            info.data = dataResp;
                            statusProfile = STATUSPROFILE.Updating;
                            if(SqliteDataAccess.UpdateDeviceRF(ip, statusProfile.ToString()))
                            {
                                mainWindowModel.ReloadListDeviceRFDGV();
                            }
                            this.Publish(publishdata, info);
                            new Thread((MainWindowModel) =>
                            {
                                int cntTimeOut = 0;

                                do
                                {
                                    if (OnFlagStatusClient.OnConfirmProfileSuccess == CLIENTCMD.CONFIRM_ADD_PROFILE_SUCCESS ||
                                        OnFlagStatusClient.OnConfirmProfileSuccess == CLIENTCMD.CONFIRM_ADD_PROFILE_UNSUCCESS ||
                                        OnFlagStatusClient.OnConfirmProfileSuccess == CLIENTCMD.CONFIRM_DELETE_PROFILE_SUCCESS ||
                                        OnFlagStatusClient.OnConfirmProfileSuccess == CLIENTCMD.CONFIRM_DELETE_PROFILE_UNSUCCESS ||
                                        OnFlagStatusClient.OnConfirmProfileSuccess == CLIENTCMD.CONFIRM_UPDATE_PROFILE_SUCCESS ||
                                        OnFlagStatusClient.OnConfirmProfileSuccess == CLIENTCMD.CONFIRM_UPDATE_PROFILE_UNSUCCESS)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(1000);
                                }
                                while (cntTimeOut++ < 120);

                                if (OnFlagStatusClient.OnConfirmProfileSuccess == CLIENTCMD.CONFIRM_ADD_PROFILE_SUCCESS ||
                                    OnFlagStatusClient.OnConfirmProfileSuccess == CLIENTCMD.CONFIRM_DELETE_PROFILE_SUCCESS ||
                                    OnFlagStatusClient.OnConfirmProfileSuccess == CLIENTCMD.CONFIRM_UPDATE_PROFILE_SUCCESS)
                                {
                                    OnFlagStatusClient.OnConfirmProfileSuccess = CLIENTCMD.CLIENT_READY;
                                    statusProfile = STATUSPROFILE.Updated;
                                    if(SqliteDataAccess.UpdateDeviceRF(ip, statusProfile.ToString() + " " + DateTime.Now.ToString("MM/dd/yyyy h:mm:ss tt")))
                                    {
                                        mainWindowModel.ReloadListDeviceRFDGV();
                                    }
                                }
                                else
                                {
                                    OnFlagStatusClient.OnConfirmProfileSuccess = CLIENTCMD.CLIENT_READY;
                                    statusProfile = STATUSPROFILE.Failed;
                                    if(SqliteDataAccess.UpdateDeviceRF(ip, statusProfile.ToString()))
                                    {
                                        mainWindowModel.ReloadListDeviceRFDGV();
                                    }
                                }

                                
                            }

                            ).Start(mainWindowModel);
                        }
                    }
                    catch (Exception ex)
                    {
                        statusProfile = STATUSPROFILE.Error;
                        if(SqliteDataAccess.UpdateDeviceRF(ip, statusProfile.ToString()))
                        {
                            mainWindowModel.ReloadListDeviceRFDGV();
                        }
                        logFile.Error(ex.Message);
                        Constant.mainWindowPointer.WriteLog(ex.Message);
                    }
                }
            }
        }

        public void sendProfileSync(string ip)
        {
            try
            {
                JStringProfile Jprofile = new JStringProfile();
                Jprofile.status = (int)SERVERRESPONSE.RESP_PROFILE_ADD;
                Jprofile.data = mainWindowModel.GetListSerialId(ip);
                string dataResp = JsonConvert.SerializeObject(Jprofile).ToString();
                StandardString info = new StandardString();
                info.data = dataResp;
                this.Publish(publishdata, info);
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }

        public void requestPersonListImmediately()
        {
            try
            {
                dynamic product = new JObject();
                product.status = SERVERRESPONSE.RESP_REQ_PERSONLIST_IMMEDIATELY;
                StandardString msg = new StandardString();
                msg.data = product.ToString();
                Publish(publishdata, msg);
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }

        private void DataHandler(Message message)
        {
            StandardString standard = (StandardString)message;
            try
            {
                JObject stuff = JObject.Parse(standard.data);
                var status = (CLIENTCMD)((int)stuff["status"]);
                var ip = (string)stuff["ip"];
                switch (status)
                {
                    case CLIENTCMD.REQUEST_PROFILE_ADD:
                        //sendProfile(ip, SERVERRESPONSE.RESP_PROFILE_ADD, new List<Profile>());
                        //dynamic product=new JOb
                        break;

                    case CLIENTCMD.REQUEST_PROFILE_UPDATE:
                        //sendProfile(ip, SERVERRESPONSE.RESP_PROFILE_UPDATE, new List<Profile>());
                        //dynamic product=new JOb
                        break;

                    case CLIENTCMD.REQUEST_PROFILE_DELETE:
                        //sendProfile(ip, SERVERRESPONSE.RESP_PROFILE_DELETE, new List<Profile>());
                        //dynamic product=new JOb
                        break;

                    case CLIENTCMD.CONFIRM_ADD_PROFILE_SUCCESS:
                        OnFlagStatusClient.OnConfirmProfileSuccess = status;
                        break;

                    case CLIENTCMD.CONFIRM_UPDATE_PROFILE_SUCCESS:
                        OnFlagStatusClient.OnConfirmProfileSuccess = status;
                        break;

                    case CLIENTCMD.CONFIRM_DELETE_PROFILE_SUCCESS:
                        OnFlagStatusClient.OnConfirmProfileSuccess = status;
                        break;

                    case CLIENTCMD.CONFIRM_ADD_PROFILE_UNSUCCESS:
                        OnFlagStatusClient.OnConfirmProfileSuccess = status;
                        break;

                    case CLIENTCMD.CONFIRM_UPDATE_PROFILE_UNSUCCESS:
                        OnFlagStatusClient.OnConfirmProfileSuccess = status;
                        break;

                    case CLIENTCMD.CONFIRM_DELETE_PROFILE_UNSUCCESS:
                        OnFlagStatusClient.OnConfirmProfileSuccess = status;
                        break;

                    case CLIENTCMD.REQUEST_SYNC_TIME:
                        dynamic productTimeSync = new JObject();
                        productTimeSync.status = (int)SERVERRESPONSE.RESP_SYNC_TIME;
                        productTimeSync.data = DateTime.Now.Ticks;
                        StandardString msgTimeSync = new StandardString();
                        msgTimeSync.data = productTimeSync.ToString();
                        Publish(publishdata, msgTimeSync);
                        break;

                    case CLIENTCMD.REQUEST_REG_PERSON_LIST:
                        List<CheckinData> personList = new List<CheckinData>();
                        try
                        {
                            JObject dataClient = JObject.Parse(standard.data);
                            if (dataClient["data"].Count() > 0)
                            {
                                foreach (var result in dataClient["data"])
                                {
                                    string serialId = (string)result["serialId"];
                                    string tick = (string)result["tick"];
                                    CheckinData person = new CheckinData() { SERIAL_ID = serialId, TIMECHECK = tick };
                                    personList.Add(person);
                                }
                                if (personList.Count > 0)
                                {
                                    mainWindowModel.CheckinServer(ip, personList);
                                }
                            }
                            try
                            {
                                dynamic product = new JObject();
                                product.status = SERVERRESPONSE.RESP_SUCCESS;
                                StandardString msg = new StandardString();
                                msg.data = product.ToString();
                                Publish(publishdata, msg);
                            }
                            catch (Exception ex)
                            {
                                logFile.Error(ex.Message);
                                Constant.mainWindowPointer.WriteLog(ex.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                dynamic product = new JObject();
                                product.status = SERVERRESPONSE.RESP_PERSONLIST_ERROR;
                                StandardString msg = new StandardString();
                                msg.data = product.ToString();
                                Publish(publishdata, msg);
                                logFile.Error(ex.Message);
                                Constant.mainWindowPointer.WriteLog(ex.Message);
                            }
                            catch (Exception exc)
                            {
                                logFile.Error(exc.Message);
                                Constant.mainWindowPointer.WriteLog(ex.Message);
                            }
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }
    }
}