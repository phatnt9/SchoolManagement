using SchoolManagement.Form;
using System.Windows;
using SchoolManagement.Model;
using System.ComponentModel;
using SchoolManagement.DTO;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace SchoolManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindowModel mainModel;

        public MainWindow()
        {
            InitializeComponent();
            Closed += MainWindow_Closed;
            mainModel = new MainWindowModel(this);
            DataContext = mainModel;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Registration_Click(object sender, RoutedEventArgs e)
        {
            RegisterForm regForm = new RegisterForm(this);
            regForm.ShowDialog();
            mainModel.ReloadListProfileRFDGV();
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            ImportForm importForm = new ImportForm();
            importForm.ShowDialog();
        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string array = "0";
                string[] test = array.Split(',');
                Console.WriteLine(test.Length);

                //DeviceRF deviceRF = DeviceRFListData.SelectedItem as DeviceRF;
                //List<string> test = SqliteDataAccess.LoadListProfileRFSerialId(deviceRF.IP);
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        private void Btn_search_Click(object sender, RoutedEventArgs e)
        {
            mainModel.ReloadListTimeCheckDGV();
        }

        private void Btn_requestListTimeCheck_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    foreach (DeviceRF device in mainModel.deviceRFList)
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

        private void Btn_sendNewListPerson_Click(object sender, RoutedEventArgs e)
        {
            //Update profile for each device
            Task.Run(() =>
            {
                try
                {
                    foreach (DeviceRF device in mainModel.deviceRFList)
                    {
                        device.deviceItem.sendProfile(device.IP);
                    }
                }
                catch (Exception ex)
                {
                    logFile.Error(ex.Message);
                }
            });
        }

        private void DataTabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.Source is System.Windows.Controls.TabControl)
            {
                switch (((e.Source as System.Windows.Controls.TabControl).SelectedIndex))
                {
                    case 0:
                        {
                            Console.WriteLine("pick tab 0");
                            break;
                        }
                    case 1:
                        {
                            Console.WriteLine("pick tab 1");
                            break;
                        }
                }
            }
        }

        private void MainTabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.Source is System.Windows.Controls.TabControl)
            {
                switch (((e.Source as System.Windows.Controls.TabControl).SelectedIndex))
                {
                    case 0:
                        {
                            mainModel.ReloadListProfileRFDGV();
                            break;
                        }
                    case 1:
                        {
                            mainModel.ReloadListDeviceRFDGV();
                            break;
                        }
                }
            }
        }

        private void AddDeviceRF_Click(object sender, RoutedEventArgs e)
        {
            AddDeviceRFForm frm = new AddDeviceRFForm();
            frm.ShowDialog();
            mainModel.ReloadListDeviceRFDGV();
        }

        private void Btn_fakeTimeCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProfileRF profileRF = AccountListData.SelectedItem as ProfileRF;
                SqliteDataAccess.SaveTimeCheckRF(profileRF.SERIAL_ID, DateTime.Now);
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        private void Btn_delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (System.Windows.Forms.MessageBox.Show
                        (
                        String.Format(Constant.messageDeleteConfirm, "Profile"),
                        Constant.messageTitileWarning, MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes
                        )
                {
                    ProfileRF profileRF = AccountListData.SelectedItem as ProfileRF;
                    if (profileRF != null)
                    {
                        SqliteDataAccess.RemoveProfileRF(profileRF);
                        mainModel.ReloadListProfileRFDGV();
                    }
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }

        }

        private void Btn_start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeviceRF deviceRF = (sender as System.Windows.Controls.Button).DataContext as DeviceRF;
                deviceRF.deviceItem.Start("ws://" + deviceRF.IP + ":9090");
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        private void Btn_stop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeviceRF deviceRF = (sender as System.Windows.Controls.Button).DataContext as DeviceRF;
                deviceRF.deviceItem.Dispose();
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        private void Btn_test_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeviceRF deviceRF = (sender as System.Windows.Controls.Button).DataContext as DeviceRF;
                List<string> test = SqliteDataAccess.LoadListProfileRFSerialId(deviceRF.IP);
                Console.WriteLine("============");
                foreach (string item in test)
                {
                    Console.WriteLine(item);
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        private void AccountListData_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            try
            {
                if (AccountListData.SelectedItem != null)
                {
                    ProfileRF temp = AccountListData.SelectedItem as ProfileRF;
                    tb_serialID.Text = temp.SERIAL_ID;
                    tb_name.Text = temp.NAME;
                    tb_dateofbirth.Text = temp.BIRTHDAY.ToLongDateString();
                    tb_student.Text = temp.STUDENT;
                    tb_class.Text = temp.CLASS;
                    tb_email.Text = temp.EMAIL;
                    tb_address.Text = temp.ADDRESS;
                    tb_phone.Text = temp.PHONE;
                    if (temp.GENDER == Constant.Gender.Male)
                    {
                        rb_male.IsChecked = true;
                    }
                    else
                    {
                        rb_female.IsChecked = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
