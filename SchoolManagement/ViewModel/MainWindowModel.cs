using Microsoft.Office.Interop.Excel;
using SchoolManagement.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;

using Excel = Microsoft.Office.Interop.Excel;

namespace SchoolManagement.ViewModel
{
    public class MainWindowModel:ViewModelBase
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public enum Mode
        {
            ADD = 0,
            UPDATE = 1,
            DELETE = 2
        }

        public enum AppStatus
        {
            Ready,
            Exporting,
            Completed,
            Finished,
            Cancelled,
            Error,
        }

        private BackgroundWorker worker;
        private bool _isModifyDataGridOpen;
        public bool IsModifyDataGridOpen { get => _isModifyDataGridOpen; set { _isModifyDataGridOpen = value; RaisePropertyChanged("IsModifyDataGridOpen"); } }
        private AppStatus _pgbStatus;
        public AppStatus PgbStatus { get => _pgbStatus; set { _pgbStatus = value; RaisePropertyChanged("PgbStatus"); } }

        public System.Timers.Timer timerSyncTimeSheet;
        public System.Timers.Timer SuspendStudentCheckTimer;
        public MainWindow mainW;

        public ListCollectionView groupedAccount { get; private set; }
        public ListCollectionView groupedTimeCheck { get; private set; }
        public ListCollectionView groupedDevice { get; private set; }

        private ObservableCollection<Profile> _profilestosend = new ObservableCollection<Profile>();
        public ObservableCollection<Profile> ProfilesToSend => _profilestosend;
        //public ICollectionView collectionView;

        public List<Profile> accountRFList;
        public List<Device> deviceRFList;
        public List<TimeRecord> timeCheckRFList;

        //public DeviceItem deviceItem;
        public MainWindowModel(MainWindow mainW)
        {
            this.mainW = mainW;
            PgbStatus = AppStatus.Ready;
            timerSyncTimeSheet = new System.Timers.Timer();
            timerSyncTimeSheet.Interval = Properties.Settings.Default.RequestTimeCheckInterval;
            timerSyncTimeSheet.Elapsed += TimerSyncTimeSheet_Elapsed;
            timerSyncTimeSheet.AutoReset = true;
            timerSyncTimeSheet.Start();

            accountRFList = new List<Profile>();
            deviceRFList = new List<Device>();
            timeCheckRFList = new List<TimeRecord>();

            groupedAccount = (ListCollectionView)CollectionViewSource.GetDefaultView(accountRFList);
            groupedTimeCheck = (ListCollectionView)CollectionViewSource.GetDefaultView(timeCheckRFList);
            groupedDevice = (ListCollectionView)CollectionViewSource.GetDefaultView(deviceRFList);
            //collectionView = CollectionViewSource.GetDefaultView(ProfilesToSend);

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
                    foreach (Device device in deviceRFList)
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

        public void CheckSuspendAllProfile()
        {
            try
            {
                //Get all Profile
                List<Profile> profiles = SqliteDataAccess.LoadProfileRF();

                //Check status --> check date to Suspend --> Suspend(active)
                foreach (Profile profile in profiles)
                {
                    if (profile.STATUS == "Active" && profile.CHECK_DATE_TO_LOCK == true)
                    {
                        if (DateTime.Now > profile.DATE_TO_LOCK)
                        {
                            profile.STATUS = "Suspended";
                            profile.LOCK_DATE = DateTime.Now;
                            SqliteDataAccess.UpdateProfileRF(profile, profile.STATUS);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }

        public void ReloadListProfileRFDGV(string name = "", string pinno = "", string adno = "")
        {
            try
            {
                accountRFList.Clear();
                List<Profile> profileList = SqliteDataAccess.LoadProfileRF(name, pinno, adno);
                foreach (Profile item in profileList)
                {
                    accountRFList.Add(item);
                }
                if (groupedAccount.IsEditingItem)
                {
                    groupedAccount.CommitEdit();
                }

                if (groupedAccount.IsAddingNew)
                {
                    groupedAccount.CommitNew();
                }

                groupedAccount.Refresh();
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }

        public bool CheckExistDeviceRF(List<Device> list, Device deviceRF)
        {
            foreach (Device item in list)
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

        public void ReloadListDeviceRFDGV(Device removedDevice = null)
        {
            mainW.DeviceRFListData.Dispatcher.BeginInvoke(new ThreadStart(() =>
             {
                 try
                 {
                     List<Device> deviceList = SqliteDataAccess.LoadDeviceRF();
                     foreach (Device item in deviceList)
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
                     {
                         groupedDevice.CommitEdit();
                     }

                     if (groupedDevice.IsAddingNew)
                     {
                         groupedDevice.CommitNew();
                     }

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
                        if (mainW.AccountListLocal.SelectedItem != null && mainW.dp_search.SelectedDate != null)
                        {
                            Profile profileRF = mainW.AccountListLocal.SelectedItem as Profile;
                            DateTime date = (DateTime)mainW.dp_search.SelectedDate;
                            List<TimeRecord> timeList = SqliteDataAccess.LoadTimeCheckRF(profileRF.PIN_NO, date);
                            foreach (TimeRecord item in timeList)
                            {
                                timeCheckRFList.Add(item);
                            }
                        }
                        else
                        {
                            if (mainW.AccountListLocal.SelectedItem != null)
                            {
                                Profile profileRF = mainW.AccountListLocal.SelectedItem as Profile;
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
                            Device deviceRF = mainW.DeviceRFListData.SelectedItem as Device;
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
                                Device deviceRF = mainW.DeviceRFListData.SelectedItem as Device;
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
                {
                    groupedTimeCheck.CommitEdit();
                }

                if (groupedTimeCheck.IsAddingNew)
                {
                    groupedTimeCheck.CommitNew();
                }

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

        public List<Profile> GetListSerialId(string ip)
        {
            try
            {
                List<Profile> returnSerialId = SqliteDataAccess.LoadListProfileRFSerialId(ip);
                return returnSerialId;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
                return new List<Profile>();
            }
        }

        public void AddProfileToModifyList(Profile item)
        {
            if (!CheckIfProfileContainInModifyList(item))
            {
                ProfilesToSend.Add(item);
            }
            else
            {
                for (int indexOfProfile = 0; indexOfProfile < ProfilesToSend.Count; indexOfProfile++)
                {
                    if (ProfilesToSend[indexOfProfile].PIN_NO == item.PIN_NO)
                    {
                        Console.WriteLine("Duplicated!-" + item.PIN_NO);
                        ProfilesToSend.RemoveAt(indexOfProfile);
                        ProfilesToSend.Add(item);
                        break;
                    }
                }
            }
        }

        public bool CheckIfProfileContainInModifyList(Profile item)
        {
            foreach (Profile profile in ProfilesToSend)
            {
                if (profile.PIN_NO == item.PIN_NO)
                {
                    return true;
                }
            }
            return false;
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

        public void DeselectedProfileFromProfilesToSend(System.Collections.IList profiles)
        {
            List<Profile> tempList = new List<Profile>();
            foreach (Profile item in profiles)
            {
                tempList.Add(item);
            }
            foreach (Profile item in tempList)
            {
                for (int indexOfProfile = 0; indexOfProfile < ProfilesToSend.Count; indexOfProfile++)
                {
                    if (ProfilesToSend[indexOfProfile].PIN_NO == item.PIN_NO)
                    {
                        Console.WriteLine("Founded!-" + item.PIN_NO);
                        ProfilesToSend.RemoveAt(indexOfProfile);
                        break;
                    }
                }
            }
        }

        public void SendProfileToDevices(Mode mode)
        {
            if (ProfilesToSend.Count > 0)
            {
                foreach (Device device in deviceRFList)
                {
                    List<Profile> listProfileToSendForThisDevice = new List<Profile>();
                    foreach (Profile profileToSend in ProfilesToSend)
                    {
                        if (device.CLASS.Contains(profileToSend.CLASS))
                        {
                            listProfileToSendForThisDevice.Add(profileToSend);
                        }
                    }
                    //Send
                    if (device.deviceItem.webSocket.IsAlive && (listProfileToSendForThisDevice.Count > 0))
                    //if ((listProfileToSendForThisDevice.Count>0))
                    {
                        DeviceItem.SERVERRESPONSE serRes;
                        switch (mode)
                        {
                            case Mode.ADD:
                            {
                                serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_ADD;
                                break;
                            }
                            case Mode.UPDATE:
                            {
                                serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_UPDATE;
                                break;
                            }
                            case Mode.DELETE:
                            {
                                serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_DELETE;
                                break;
                            }
                            default:
                            {
                                serRes = DeviceItem.SERVERRESPONSE.RESP_PROFILE_ADD;
                                break;
                            }
                        }
                        device.deviceItem.sendProfile(device.IP, serRes, listProfileToSendForThisDevice);
                    }
                }
                ProfilesToSend.Clear();
            }
        }

        public void OpenModifyDataGrid()
        {
            try
            {

            }
            catch
            {

            }
            finally
            {
                if (!IsModifyDataGridOpen)
                {
                    mainW.DGV_ModilyList.Width = new GridLength(1, GridUnitType.Star);
                    mainW.LocalList_ButtonsGrid.Height = mainW.SendList_ButtonsGrid.Height = new GridLength(30);
                    mainW.StackButton_ProfileAddUpdateRemove.Height = new GridLength(35);
                    IsModifyDataGridOpen = true;
                }
            }
        }

        public void CloseModifyDataGrid()
        {
            try
            {
                ProfilesToSend.Clear();
            }
            catch
            {

            }
            finally
            {
                IsModifyDataGridOpen = false;
                mainW.DGV_ModilyList.Width =
                    mainW.LocalList_ButtonsGrid.Height =
                    mainW.SendList_ButtonsGrid.Height =
                    mainW.StackButton_ProfileAddUpdateRemove.Height = new GridLength(0);
            }
        }

        public void ExportAllProfile()
        {
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.ProgressChanged += Worker_ProgressChanged;

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            saveDialog.FilterIndex = 2;
            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                worker.RunWorkerAsync(argument: saveDialog);
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            mainW.pbStatus.Value = e.ProgressPercentage;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // handle the error
                PgbStatus = AppStatus.Error;
            }
            else if (e.Cancelled)
            {
                // handle cancellation
                PgbStatus = AppStatus.Cancelled;
            }
            else
            {
                PgbStatus = AppStatus.Completed;
            }
            // general cleanup code, runs when there was an error or not.
            mainW.pbStatus.Value = 0;
            PgbStatus = AppStatus.Ready;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            PgbStatus = AppStatus.Exporting;
            Excel.Application excel = new Excel.Application();
            Excel.Workbook workbook = excel.Workbooks.Add(Type.Missing);
            Excel.Worksheet worksheet = null;

            try
            {
                SaveFileDialog saveDialog = (SaveFileDialog)e.Argument;

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
                worksheet.Cells[1, 15] = "Expire Date";
                worksheet.Cells[1, 16] = "Automatic Suspension";

                int cellRowIndex = 2;
                int cellColumnIndex = 1;

                List<Profile> profileList = SqliteDataAccess.LoadProfileRF();

                for (int i = 0; i < profileList.Count; i++)
                {
                    for (int j = 0; j < 16; j++)
                    {
                        if (j == 0)//No
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = i + 1; }
                        if (j == 1)//Name
                        { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].NAME; }
                        if (j == 2)//Adno
                        {
                            Range cells = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                            cells.NumberFormat = "@";
                            cells.HorizontalAlignment = XlHAlign.xlHAlignRight;
                            cells.Value2 = profileList[i].ADNO;
                            //worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].ADNO;
                        }
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
                        {
                            Range cells = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                            cells.NumberFormat = "@";
                            cells.HorizontalAlignment = XlHAlign.xlHAlignRight;
                            cells.Value2 = profileList[i].IMAGE;

                            //worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].IMAGE;
                        }
                        if (j == 7)//PIN No.
                        {
                            Range cells = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                            cells.NumberFormat = "@";
                            cells.HorizontalAlignment = XlHAlign.xlHAlignRight;
                            cells.Value2 = profileList[i].PIN_NO;
                            //worksheet.Range[cellColumnIndex].NumberFormat = "0";
                            //worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].PIN_NO;
                        }

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
                            list.Add("Security");
                            list.Add("Admin");
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
                        {
                            Range cells = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                            cells.NumberFormat = "@";
                            cells.HorizontalAlignment = XlHAlign.xlHAlignRight;
                            cells.Value2 = profileList[i].PHONE;
                            //worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].PHONE;
                        }
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

                        if (j == 14)//Expire Date
                        {
                            if (profileList[i].CHECK_DATE_TO_LOCK == true)
                            {
                                { worksheet.Cells[cellRowIndex, cellColumnIndex] = profileList[i].DATE_TO_LOCK; }
                            }
                            else
                            {
                                { worksheet.Cells[cellRowIndex, cellColumnIndex] = ""; }
                            }
                        }
                        if (j == 15)// Automatic Suspension
                        {
                            var list = new System.Collections.Generic.List<string>();
                            list.Add("TRUE");
                            list.Add("FALSE");
                            var flatList = string.Join(",", list.ToArray());

                            var cell = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[cellRowIndex, cellColumnIndex];
                            cell.Validation.Delete();
                            cell.Validation.Add(
                               Excel.XlDVType.xlValidateList,
                               Excel.XlDVAlertStyle.xlValidAlertInformation,
                               Excel.XlFormatConditionOperator.xlBetween,
                               flatList,
                               Type.Missing);
                            cell.Value2 = profileList[i].CHECK_DATE_TO_LOCK;
                            cell.Locked = true;
                            cell.Validation.IgnoreBlank = true;
                            cell.Validation.InCellDropdown = true;
                        }

                        cellColumnIndex++;
                    }
                    cellColumnIndex = 1;
                    cellRowIndex++;
                    (sender as BackgroundWorker).ReportProgress((i * 100) / profileList.Count);
                }

                worksheet.Columns.AutoFit();
                workbook.SaveAs(saveDialog.FileName, Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing, false, false, Excel.XlSaveAsAccessMode.xlNoChange, Excel.XlSaveConflictResolution.xlLocalSessionChanges, Type.Missing, Type.Missing);
                System.Windows.Forms.MessageBox.Show("Export Successful");
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