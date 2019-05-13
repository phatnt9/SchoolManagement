using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Forms;
using System.Threading;
using SchoolManagement.DTO;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading.Tasks;

namespace SchoolManagement.Model
{
    public class MainWindowModel
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public System.Timers.Timer timerSyncTimeSheet;
        public MainWindow mainW;
        
        public ListCollectionView groupedAccount { get; private set; }
        public ListCollectionView groupedTimeCheck { get; private set; }
        public ListCollectionView groupedDevice { get; private set; }
        
        
        public List<ProfileRF> accountRFList;
        public List<DeviceRF> deviceRFList;
        public List<TimeRecord> timeCheckRFList;

        //public DeviceItem deviceItem;
        public MainWindowModel(MainWindow mainW)
        {
            
            this.mainW = mainW;

            timerSyncTimeSheet = new System.Timers.Timer();
            timerSyncTimeSheet.Interval = Properties.Settings.Default.RequestTimeCheckInterval;
            timerSyncTimeSheet.Elapsed += TimerSyncTimeSheet_Elapsed;
            timerSyncTimeSheet.AutoReset = true;
            timerSyncTimeSheet.Start();

            accountRFList = new List<ProfileRF>();
            deviceRFList = new List<DeviceRF>();
            timeCheckRFList = new List<TimeRecord>();

            groupedAccount = (ListCollectionView)CollectionViewSource.GetDefaultView(accountRFList);
            groupedTimeCheck = (ListCollectionView)CollectionViewSource.GetDefaultView(timeCheckRFList);
            groupedDevice = (ListCollectionView)CollectionViewSource.GetDefaultView(deviceRFList);

            //DeviceItem deviceItem = new DeviceItem(this);
            //deviceItem.Start("ws://192.168.1.121:9090");

        }

        private void TimerSyncTimeSheet_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("TimerSyncTimeSheet_Elapsed");
            Task.Run(() =>
            {
                try
                {
                    foreach (DeviceRF device in deviceRFList)
                    {
                        device.deviceItem.requestPersonListImmediately();
                    }
                }
                catch (Exception ex)
                {
                    logFile.Error(ex.Message);
                    Constant.mainWindowPointer.WriteLog(ex.Message);
                }
            });
        }

        public void ReloadListProfileRFDGV(string name = "", string pinno = "", string adno = "")
        {
            try
            {
                accountRFList.Clear();
                List<ProfileRF> profileList = SqliteDataAccess.LoadProfileRF(name, pinno, adno);
                foreach (ProfileRF item in profileList)
                {
                    accountRFList.Add(item);
                }
                if (groupedAccount.IsEditingItem)
                    groupedAccount.CommitEdit();
                if (groupedAccount.IsAddingNew)
                    groupedAccount.CommitNew();
                groupedAccount.Refresh();
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }

        }

        public bool CheckExistDeviceRF (List<DeviceRF> list, DeviceRF deviceRF)
        {
            foreach (DeviceRF item in list)
            {
                if ((item.IP == deviceRF.IP))
                {
                    item.STATUS = deviceRF.STATUS;
                    item.GATE = deviceRF.GATE;
                    item.CLASS = deviceRF.CLASS;
                    return true;
                }
            }
            return false;
        }

        public void ReloadListDeviceRFDGV(DeviceRF removedDevice = null)
        {
           mainW.DeviceRFListData.Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                try
                {
                    List<DeviceRF> deviceList = SqliteDataAccess.LoadDeviceRF();
                    foreach (DeviceRF item in deviceList)
                    {
                        if (!CheckExistDeviceRF(deviceRFList, item))
                        {
                            item.deviceItem = new DeviceItem(this);
                            deviceRFList.Add(item);
                        }
                    }
                    if (removedDevice != null)
                    {
                        deviceRFList.Remove(removedDevice);
                    }


                    if (groupedDevice.IsEditingItem)
                        groupedDevice.CommitEdit();
                    if (groupedDevice.IsAddingNew)
                        groupedDevice.CommitNew();
                    groupedDevice.Refresh();
                }
                catch (Exception ex)
                {
                    logFile.Error(ex.Message);
                    Constant.mainWindowPointer.WriteLog(ex.Message);
                }
            }));
            
        }

        public void ReloadListTimeCheckDGV(int tabIndex)
        {
            try
            {
                timeCheckRFList.Clear();
                switch (tabIndex)
                {
                    case 0: // tab profile
                        {
                            if (mainW.AccountListData.SelectedItem != null && mainW.dp_search.SelectedDate != null)
                            {
                                ProfileRF profileRF = mainW.AccountListData.SelectedItem as ProfileRF;
                                DateTime date = (DateTime)mainW.dp_search.SelectedDate;
                                List<TimeRecord> timeList = SqliteDataAccess.LoadTimeCheckRF(profileRF.PIN_NO, date);
                                foreach (TimeRecord item in timeList)
                                {
                                    timeCheckRFList.Add(item);
                                }
                            }
                            else
                            {
                                if (mainW.AccountListData.SelectedItem != null)
                                {
                                    ProfileRF profileRF = mainW.AccountListData.SelectedItem as ProfileRF;
                                    List<TimeRecord> timeList = SqliteDataAccess.LoadTimeCheckRF(profileRF.PIN_NO, DateTime.MinValue);
                                    foreach (TimeRecord item in timeList)
                                    {
                                        timeCheckRFList.Add(item);
                                    }
                                }
                            }
                            break;
                        }
                    case 1: // tab device
                        {
                            if (mainW.DeviceRFListData.SelectedItem != null && mainW.dp_search.SelectedDate != null)
                            {
                                DeviceRF deviceRF = mainW.DeviceRFListData.SelectedItem as DeviceRF;
                                DateTime date = (DateTime)mainW.dp_search.SelectedDate;
                                List<TimeRecord> timeList = SqliteDataAccess.LoadTimeCheckRF("", date, deviceRF.IP);
                                foreach (TimeRecord item in timeList)
                                {
                                    timeCheckRFList.Add(item);
                                }
                            }
                            else
                            {
                                if (mainW.DeviceRFListData.SelectedItem != null)
                                {
                                    DeviceRF deviceRF = mainW.DeviceRFListData.SelectedItem as DeviceRF;
                                    List<TimeRecord> timeList = SqliteDataAccess.LoadTimeCheckRF("", DateTime.MinValue, deviceRF.IP);
                                    foreach (TimeRecord item in timeList)
                                    {
                                        timeCheckRFList.Add(item);
                                    }
                                }
                            }
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
                

                if (groupedTimeCheck.IsEditingItem)
                    groupedTimeCheck.CommitEdit();
                if (groupedTimeCheck.IsAddingNew)
                    groupedTimeCheck.CommitNew();
                groupedTimeCheck.Refresh();
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }
        
        public bool CheckinServer(string ip, List<CheckinData> person)
        {
            try
            {
                foreach (CheckinData p in person)
                {
                    string[] timeArray = p.TIMECHECK.Split(';');
                    foreach (string tick in timeArray)
                    {
                        long date = long.Parse(tick);
                        DateTime dateTime = new DateTime(date);
                        SqliteDataAccess.SaveTimeCheckRF(ip, p.SERIAL_ID, dateTime);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
                return false;
            }
            finally
            {
                
            }

            
        }

        public List<string> GetListSerialId(string ip)
        {
            try
            {
                List<string> returnSerialId = SqliteDataAccess.LoadListProfileRFSerialId(ip);
                return returnSerialId;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
                return new List<string>();
            }
        }

        public void ExportListTimeCheck()
        {
            if (timeCheckRFList.Count <= 0)
            {
                return;
            }
            Excel.Application excel = new Excel.Application();
            Excel.Workbook workbook = excel.Workbooks.Add(Type.Missing);
            Excel.Worksheet worksheet = null;

            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                saveDialog.FilterIndex = 2;
                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    excel.DisplayAlerts = false;
                    worksheet = workbook.ActiveSheet;

                    worksheet.Cells[1, 1] = "No";
                    worksheet.Cells[1, 2] = "PIN No.";
                    worksheet.Cells[1, 3] = "Adno";
                    worksheet.Cells[1, 4] = "Name";
                    worksheet.Cells[1, 5] = "Gate";
                    worksheet.Cells[1, 6] = "Time";

                    int cellRowIndex = 2;
                    int cellColumnIndex = 1;
                    

                    for (int i = 0; i < timeCheckRFList.Count; i++)
                    {
                        for (int j = 0; j < 6; j++)
                        {
                            if (j == 0)//No
                            { worksheet.Cells[cellRowIndex, cellColumnIndex] = i + 1; }
                            if (j == 1)//PIN No.
                            { worksheet.Cells[cellRowIndex, cellColumnIndex] = timeCheckRFList[i].PIN_NO; }
                            if (j == 2)//Adno
                            { worksheet.Cells[cellRowIndex, cellColumnIndex] = timeCheckRFList[i].ADNO; }
                            if (j == 3)//Name
                            { worksheet.Cells[cellRowIndex, cellColumnIndex] = timeCheckRFList[i].NAME; }
                            if (j == 4)//Gate
                            { worksheet.Cells[cellRowIndex, cellColumnIndex] = timeCheckRFList[i].GATE.ToString(); }
                            if (j == 5)//Time
                            { worksheet.Cells[cellRowIndex, cellColumnIndex] = timeCheckRFList[i].TIME_CHECK.ToString("MM/dd/yyyy HH:mm:ss"); }
                            
                            cellColumnIndex++;
                        }
                        cellColumnIndex = 1;
                        cellRowIndex++;
                    }


                    worksheet.Columns.AutoFit();

                    workbook.SaveAs(saveDialog.FileName, Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing, false, false, Excel.XlSaveAsAccessMode.xlNoChange, Excel.XlSaveConflictResolution.xlLocalSessionChanges, Type.Missing, Type.Missing);
                    System.Windows.Forms.MessageBox.Show("Export Successful");
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
            finally
            {
                excel.Quit();
                workbook = null;
                excel = null;
            }
        }

        public void ExportAllProfile()
        {
            Excel.Application excel = new Excel.Application();
            Excel.Workbook workbook = excel.Workbooks.Add(Type.Missing);
            Excel.Worksheet worksheet = null;

            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                saveDialog.FilterIndex = 2;
                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    excel.DisplayAlerts = false;
                    worksheet = workbook.ActiveSheet;

                    worksheet.Cells[1, 1] = "No";
                    worksheet.Cells[1, 2] = "Name";
                    worksheet.Cells[1, 3] = "Adno";
                    worksheet.Cells[1, 4] = "Gender";
                    worksheet.Cells[1, 5] = "DOB";
                    worksheet.Cells[1, 6] = "Disu";
                    worksheet.Cells[1, 7] = "Image";
                    worksheet.Cells[1, 8] = "PIN No.";
                    worksheet.Cells[1, 9] = "Class";
                    worksheet.Cells[1, 10] = "Email";
                    worksheet.Cells[1, 11] = "Address";
                    worksheet.Cells[1, 12] = "Phone";
                    worksheet.Cells[1, 13] = "Status";
                    worksheet.Cells[1, 14] = "Suspended Date";

                    int cellRowIndex = 2;
                    int cellColumnIndex = 1;

                    List<ProfileRF> profileList = SqliteDataAccess.LoadProfileRF();

                    for (int i=0; i < profileList.Count; i++)
                    {
                        for (int j = 0; j < 14; j++)
                        {
                            if (j == 0)//No
                            { worksheet.Cells[cellRowIndex, cellColumnIndex] = i + 1; }
                            if (j == 1)//Name
                            { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].NAME; }
                            if (j == 2)//Adno
                            { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].ADNO; }
                            if (j == 3)//Gender
                            {
                                //worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].GENDER.ToString();

                                var list = new System.Collections.Generic.List<string>();
                                list.Add("Male");
                                list.Add("Female");
                                var flatList = string.Join(",", list.ToArray());

                                var cell = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                                cell.Validation.Delete();
                                cell.Validation.Add(
                                   Excel.XlDVType.xlValidateList,
                                   Excel.XlDVAlertStyle.xlValidAlertInformation,
                                   Excel.XlFormatConditionOperator.xlBetween,
                                   flatList,
                                   Type.Missing);
                                cell.Value2 = profileList[i].GENDER.ToString();
                                cell.Locked = true;
                                cell.Validation.IgnoreBlank = true;
                                cell.Validation.InCellDropdown = true;
                            }
                            if (j == 4)//DOB
                            { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].DOB; }
                            if (j == 5)//Disu
                            { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].DISU; }
                            if (j == 6)//Image
                            { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].IMAGE; }
                            if (j == 7)//PIN No.
                            { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].PIN_NO; }


                            if (j == 8)//Class
                            {
                                //worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].CLASS;

                                var list = new System.Collections.Generic.List<string>();
                                list.Add("Staff");
                                list.Add("Parent");
                                list.Add("Student");
                                list.Add("Visitor");
                                list.Add("Long Term Supplier");
                                list.Add("Short Term Supplier");
                                var flatList = string.Join(",", list.ToArray());

                                var cell = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                                cell.Validation.Delete();
                                cell.Validation.Add(
                                   Excel.XlDVType.xlValidateList,
                                   Excel.XlDVAlertStyle.xlValidAlertInformation,
                                   Excel.XlFormatConditionOperator.xlBetween,
                                   flatList,
                                   Type.Missing);
                                cell.Value2 = profileList[i].CLASS;
                                cell.Validation.IgnoreBlank = true;
                                cell.Validation.InCellDropdown = true;
                            }





                            if (j == 9)//Email
                            { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].EMAIL; }
                            if (j == 10)//Address
                            { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].ADDRESS; }
                            if (j == 11)//Phone
                            { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].PHONE; }
                            if (j == 12)//Status
                            {
                                //worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].STATUS;

                                var list = new System.Collections.Generic.List<string>();
                                list.Add("Active");
                                list.Add("Suspended");
                                var flatList = string.Join(",", list.ToArray());

                                var cell = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                                cell.Validation.Delete();
                                cell.Validation.Add(
                                   Excel.XlDVType.xlValidateList,
                                   Excel.XlDVAlertStyle.xlValidAlertInformation,
                                   Excel.XlFormatConditionOperator.xlBetween,
                                   flatList,
                                   Type.Missing);
                                cell.Value2 = profileList[i].STATUS;
                                cell.Locked = true;
                                cell.Validation.IgnoreBlank = true;
                                cell.Validation.InCellDropdown = true;
                            }
                            if (j == 13)//Suspended Date
                            {
                                if (profileList[i].STATUS == "Suspended")
                                {
                                    { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].LOCK_DATE; }
                                }
                                else
                                {
                                    { worksheet.Cells[cellRowIndex, cellColumnIndex] = ""; }
                                }
                            }

                            cellColumnIndex++;
                        }
                        cellColumnIndex = 1;
                        cellRowIndex++;
                    }


                    worksheet.Columns.AutoFit();

                    workbook.SaveAs(saveDialog.FileName, Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing, false, false, Excel.XlSaveAsAccessMode.xlNoChange, Excel.XlSaveConflictResolution.xlLocalSessionChanges, Type.Missing, Type.Missing);
                    System.Windows.Forms.MessageBox.Show("Export Successful");
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
            finally
            {
                excel.Quit();
                workbook = null;
                excel = null;
            }
        }
        
    }
}
