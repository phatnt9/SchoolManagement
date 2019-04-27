using SchoolManagement.Form;
using System.Windows;
using SchoolManagement.Model;
using System.ComponentModel;
using SchoolManagement.DTO;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Windows.Controls;

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
        
        private void AddDeviceRF_Click(object sender, RoutedEventArgs e)
        {
            AddDeviceRFForm frm = new AddDeviceRFForm(this);
            frm.ShowDialog();
            mainModel.ReloadListDeviceRFDGV();
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
                if (AccountListData.SelectedItem == null)
                {
                    return;
                }
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
                    dp_dateofbirth.Text = temp.BIRTHDAY.ToLongDateString();
                    tb_student.Text = temp.STUDENT;
                    cbb_class.Text = temp.CLASS;
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
        

        private bool DisableEditProfile()
        {
            try
            {
                dp_dateofbirth.IsEnabled = rb_male.IsEnabled = rb_female.IsEnabled = cbb_class.IsEnabled = false;
                tb_address.IsReadOnly =
                    tb_email.IsReadOnly =
                    tb_name.IsReadOnly =
                    tb_phone.IsReadOnly =
                    tb_student.IsReadOnly =
                    cbb_class.IsReadOnly = true;
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }
        }

        private bool EnableEditProfile()
        {
            try
            {
                dp_dateofbirth.IsEnabled = rb_male.IsEnabled = rb_female.IsEnabled = cbb_class.IsEnabled = true;
                tb_address.IsReadOnly = 
                    tb_email.IsReadOnly = 
                    tb_name.IsReadOnly = 
                    tb_phone.IsReadOnly = 
                    tb_student.IsReadOnly = 
                    cbb_class.IsReadOnly = false;
                tb_name.Focus();
                return true;
            }
            catch (Exception ex)
            {
                DisableEditProfile();
                logFile.Error(ex.Message);
                return false;
            }
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            SettingForm form = new SettingForm(this);
            form.ShowDialog();
        }

        private void Btn_about_Click(object sender, RoutedEventArgs e)
        {
            About frm = new About();
            frm.ShowDialog();
        }

        private void Btn_edit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AccountListData.SelectedItem == null)
                {
                    return;
                }
                if (EnableEditProfile())
                {
                    edit.IsEnabled = false;
                    save.Visibility = Visibility.Visible;
                    AccountListData.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        private void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (String.IsNullOrEmpty(tb_name.Text.ToString()) || tb_name.Text.ToString().Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "Name", "Name"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.tb_name.Focus();
                    return;
                }

                if (String.IsNullOrEmpty(cbb_class.Text.ToString()) || cbb_class.Text.ToString().Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "cbb_class", "cbb_class"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.cbb_class.Focus();
                    return;
                }

                if (String.IsNullOrEmpty(dp_dateofbirth.Text.ToString()) || dp_dateofbirth.Text.ToString().Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "dp_dateofbirth", "dp_dateofbirth"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.dp_dateofbirth.Focus();
                    return;
                }

                if(cbb_class.Text.ToString() == "Student")
                {
                    if (String.IsNullOrEmpty(tb_student.Text.ToString()) || tb_student.Text.ToString().Trim() == "")
                    {
                        System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "tb_studentName", "tb_studentName"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        this.tb_student.Focus();
                        return;
                    }
                }

                if (String.IsNullOrEmpty(tb_email.Text.ToString()) || tb_email.Text.ToString().Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "tb_email", "tb_email"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.tb_email.Focus();
                    return;
                }

                if (String.IsNullOrEmpty(tb_address.Text.ToString()) || tb_address.Text.ToString().Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "tb_address", "tb_address"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.tb_address.Focus();
                    return;
                }

                if (String.IsNullOrEmpty(tb_phone.Text.ToString()) || tb_phone.Text.ToString().Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "tb_phone", "tb_phone"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.tb_phone.Focus();
                    return;
                }
                if (DisableEditProfile())
                {
                    ProfileRF person = new ProfileRF();
                    person.SERIAL_ID = tb_serialID.Text;
                    person.NAME = tb_name.Text;
                    person.GENDER = ((bool)rb_male.IsChecked) ? Constant.Gender.Male : Constant.Gender.Female;
                    person.CLASS = cbb_class.Text;
                    person.BIRTHDAY = (DateTime)dp_dateofbirth.SelectedDate;
                    if (person.CLASS == "Student")
                    {
                        person.STUDENT = tb_student.Text;
                    }
                    else
                    {
                        person.STUDENT = "";
                    }
                    
                    person.EMAIL = tb_email.Text;
                    person.ADDRESS = tb_address.Text;
                    person.PHONE = tb_phone.Text;
                    try
                    {
                        SqliteDataAccess.UpdateProfileRF(person);
                        mainModel.ReloadListProfileRFDGV();
                        edit.IsEnabled = true;
                        save.Visibility = Visibility.Hidden;
                        AccountListData.IsEnabled = true;
                    }
                    catch (Exception ex)
                    {
                        logFile.Error(ex.Message);
                        edit.IsEnabled = true;
                        save.Visibility = Visibility.Hidden;
                        AccountListData.IsEnabled = true;
                    }

                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                edit.IsEnabled = true;
                save.Visibility = Visibility.Hidden;
                AccountListData.IsEnabled = true;
            }
        }

        private void Cbb_class_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if ((cbb_class.SelectedItem as ComboBoxItem).Content.ToString() != "Student")
                {
                    tb_student.Text = "";
                    tb_student.IsEnabled = false;
                }
                else
                {
                    tb_student.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
            
        }

        private void DeleleDeviceRF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DeviceRFListData.SelectedItem == null)
                {
                    return;
                }
                if (System.Windows.Forms.MessageBox.Show
                        (
                        String.Format(Constant.messageDeleteConfirm, "Device"),
                        Constant.messageTitileWarning, MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes
                        )
                {
                    DeviceRF deviceRF = DeviceRFListData.SelectedItem as DeviceRF;
                    if (deviceRF != null)
                    {
                        deviceRF.deviceItem.Dispose();
                        SqliteDataAccess.RemoveDeviceRF(deviceRF);
                        mainModel.ReloadListDeviceRFDGV(deviceRF);
                    }
                }

            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        private void Btn_export_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
