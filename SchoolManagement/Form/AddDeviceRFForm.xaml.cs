﻿using SchoolManagement.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static SchoolManagement.Constant;

namespace SchoolManagement.Form
{
    /// <summary>
    /// Interaction logic for AddDeviceRFForm.xaml
    /// </summary>
    public partial class AddDeviceRFForm : Window
    {

        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        MainWindow mainW;

        public AddDeviceRFForm(MainWindow mainW)
        {
            InitializeComponent();
            this.mainW = mainW;
        }

        public AddDeviceRFForm(MainWindow mainW, DeviceRF deviceRF)
        {
            InitializeComponent();
            this.mainW = mainW;
            Title = "Edit Device";
            btn_addSave.Content = "Save";
            tb_gate.Text = deviceRF.GATE;
            tb_ip.Text = deviceRF.IP;
            string[] classArray = deviceRF.CLASS.Split(',');
            foreach (string item in classArray)
            {
                if (item.Equals("Teacher")) { cb_teacher.IsChecked = true;}
                if (item.Equals("Security")) { cb_security.IsChecked = true;}
                if (item.Equals("Student")) { cb_student.IsChecked = true;}
                if (item.Equals("Guest")) { cb_guest.IsChecked = true;}
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (String.IsNullOrEmpty(tb_ip.Text.ToString()) || tb_ip.Text.ToString().Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "tb_ip", "tb_ip"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.tb_ip.Focus();
                    return;
                }
                if ((!(bool)cb_teacher.IsChecked) && (!(bool)cb_security.IsChecked) && (!(bool)cb_student.IsChecked) && (!(bool)cb_guest.IsChecked))
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "cbb_class", "cbb_class"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //this.cbb_class.Focus();
                    return;
                }

                DeviceRF deviceRF = new DeviceRF();
                deviceRF.GATE = tb_gate.Text;
                deviceRF.IP = tb_ip.Text;
                deviceRF.STATUS = "Pending";
                string classArray = 
                    ((bool)cb_teacher.IsChecked ? "Teacher" : "") + 
                    ((bool)cb_security.IsChecked ? ",Security" : "") + 
                    ((bool)cb_student.IsChecked ? ",Student" : "") + 
                    ((bool)cb_guest.IsChecked ? ",Guest" : "");

                //Teacher = 0,
                //Security = 1,
                //Student = 2,
                //Guest = 3

                //deviceRF.CLASS = (AccountClass)cbb_class.SelectedIndex;
                deviceRF.CLASS = classArray;

                if (Title.Equals("Edit Device"))
                {
                    SqliteDataAccess.UpdateDeviceRF(deviceRF.IP, "", deviceRF.CLASS, deviceRF.GATE);
                    mainW.mainModel.ReloadListDeviceRFDGV();
                    Close();
                }
                else
                {

                    SqliteDataAccess.SaveDeviceRF(deviceRF);
                }
                lb_status.Content = "New Device Added";
                ClearForm();
                mainW.mainModel.ReloadListDeviceRFDGV();
            }
            catch (Exception ex)
            {
                lb_status.Content = "Error add new device";
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
            
        }

        public void ClearForm()
        {
            tb_ip.Clear();
            cb_guest.IsChecked = cb_security.IsChecked = cb_student.IsChecked = cb_teacher.IsChecked = false;
        }
    }
}
