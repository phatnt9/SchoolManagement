﻿using System;
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
        public List<DateTime> timeCheckRFList;

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
            timeCheckRFList = new List<DateTime>();

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
                }
            });
        }

        public void ReloadListProfileRFDGV()
        {
            try
            {
                accountRFList.Clear();
                List<ProfileRF> profileList = SqliteDataAccess.LoadProfileRF();
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
            }

        }

        public bool CheckExistDeviceRF (List<DeviceRF> list, DeviceRF deviceRF)
        {
            foreach (DeviceRF item in list)
            {
                if ((item.IP == deviceRF.IP))
                {
                    return true;
                }
            }
            return false;
        }

        public void ReloadListDeviceRFDGV(DeviceRF removedDevice = null)
        {
            try
            {
                List<DeviceRF> deviceList = SqliteDataAccess.LoadDeviceRF();
                foreach (DeviceRF item in deviceList)
                {
                    if(!CheckExistDeviceRF(deviceRFList, item))
                    {
                        DeviceItem deviceItem = new DeviceItem(this);
                        item.deviceItem = deviceItem;
                        deviceRFList.Add(item);
                    }
                }
                if(removedDevice != null)
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
            }
        }

        public void ReloadListTimeCheckDGV()
        {
            try
            {
                timeCheckRFList.Clear();
                if (mainW.AccountListData.SelectedItem != null && mainW.dp_search.SelectedDate != null)
                {
                    ProfileRF profileRF = mainW.AccountListData.SelectedItem as ProfileRF;
                    DateTime date = (DateTime)mainW.dp_search.SelectedDate;
                    List<DateTime> timeList = SqliteDataAccess.LoadTimeCheckRF(profileRF.PIN_NO, date);
                    foreach (DateTime item in timeList)
                    {
                        timeCheckRFList.Add(item);
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
            }
        }
        
        public bool CheckinServer(List<CheckinData> person)
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
                        SqliteDataAccess.SaveTimeCheckRF(p.SERIAL_ID, dateTime);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
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
                return new List<string>();
            }
        }
        
    }
}
