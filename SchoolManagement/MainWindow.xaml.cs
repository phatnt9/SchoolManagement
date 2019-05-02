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
using System.Windows.Input;

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
            ImportForm importForm = new ImportForm(this);
            importForm.ShowDialog();
            mainModel.ReloadListProfileRFDGV();
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
            mainModel.ReloadListTimeCheckDGV(MainTabControl.SelectedIndex);
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
            System.Windows.Forms.MessageBox.Show("All deviceItems updated a new profile table. Please check and ensure them to successfully updated in the DeviceItem Tab!");
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


        //private void Btn_fakeTimeCheck_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        ProfileRF profileRF = AccountListData.SelectedItem as ProfileRF;
        //        SqliteDataAccess.SaveTimeCheckRF(profileRF.PIN_NO, DateTime.Now);
        //    }
        //    catch (Exception ex)
        //    {
        //        logFile.Error(ex.Message);
        //    }
        //}

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

                    if(temp.STATUS == "Active")
                    {
                        btn_changestatuslb.Content = "Suspend Profile";
                    }
                    else
                    {
                        btn_changestatuslb.Content = "Active Profile";
                    }



                    tb_serialID.Text = temp.PIN_NO;
                    tb_adno.Text = temp.ADNO;
                    tb_name.Text = temp.NAME;
                    dp_dateofbirth.Text = temp.DOB.ToLongDateString();
                    dp_disu.Text = temp.DISU.ToLongDateString();
                    cbb_class.Text = temp.CLASS;
                    //cbb_status.Text = temp.STATUS;
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
                dp_dateofbirth.IsEnabled = 
                    dp_disu.IsEnabled = 
                    rb_male.IsEnabled = 
                    rb_female.IsEnabled =
                    //cbb_status.IsEnabled = 
                    cbb_class.IsEnabled = false;
                
                tb_address.IsReadOnly =
                    tb_email.IsReadOnly =
                    tb_adno.IsReadOnly =
                    tb_name.IsReadOnly =
                    tb_phone.IsReadOnly =
                    //tb_student.IsReadOnly =
                    //cbb_status.IsReadOnly =
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
                dp_dateofbirth.IsEnabled = 
                    dp_disu.IsEnabled = 
                    rb_male.IsEnabled = 
                    rb_female.IsEnabled = 
                    //cbb_status.IsEnabled = 
                    cbb_class.IsEnabled = true;
                tb_address.IsReadOnly = 
                    tb_email.IsReadOnly = 
                    tb_adno.IsReadOnly = 
                    tb_name.IsReadOnly = 
                    tb_phone.IsReadOnly = 
                    //tb_student.IsReadOnly = 
                    //cbb_status.IsReadOnly = 
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
                    editProfile.IsEnabled = false;
                    MainTabControl.IsEnabled = false;
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

                //if(cbb_class.Text.ToString() == "Student")
                //{
                //    if (String.IsNullOrEmpty(tb_student.Text.ToString()) || tb_student.Text.ToString().Trim() == "")
                //    {
                //        System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "tb_studentName", "tb_studentName"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //        this.tb_student.Focus();
                //        return;
                //    }
                //}

                //if (String.IsNullOrEmpty(tb_email.Text.ToString()) || tb_email.Text.ToString().Trim() == "")
                //{
                //    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "tb_email", "tb_email"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    this.tb_email.Focus();
                //    return;
                //}

                //if (String.IsNullOrEmpty(tb_address.Text.ToString()) || tb_address.Text.ToString().Trim() == "")
                //{
                //    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "tb_address", "tb_address"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    this.tb_address.Focus();
                //    return;
                //}

                //if (String.IsNullOrEmpty(tb_phone.Text.ToString()) || tb_phone.Text.ToString().Trim() == "")
                //{
                //    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "tb_phone", "tb_phone"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    this.tb_phone.Focus();
                //    return;
                //}

                if (DisableEditProfile())
                {
                    ProfileRF person = new ProfileRF();
                    person.PIN_NO = tb_serialID.Text;
                    person.ADNO = tb_adno.Text;
                    person.NAME = tb_name.Text;
                    person.GENDER = ((bool)rb_male.IsChecked) ? Constant.Gender.Male : Constant.Gender.Female;
                    person.CLASS = cbb_class.Text;
                    //person.STATUS = cbb_status.Text;
                    person.DOB = (DateTime)dp_dateofbirth.SelectedDate;
                    person.DISU = (DateTime)dp_disu.SelectedDate;
                    //if (person.CLASS == "Student")
                    //{
                    //    person.STUDENT = tb_student.Text;
                    //}
                    //else
                    //{
                    //    person.STUDENT = "";
                    //}
                    person.EMAIL = tb_email.Text;
                    person.ADDRESS = tb_address.Text;
                    person.PHONE = tb_phone.Text;
                    try
                    {
                        SqliteDataAccess.UpdateProfileRF(person);
                        mainModel.ReloadListProfileRFDGV();
                        editProfile.IsEnabled = true;
                        MainTabControl.IsEnabled = true;
                        save.Visibility = Visibility.Hidden;
                        AccountListData.IsEnabled = true;
                    }
                    catch (Exception ex)
                    {
                        logFile.Error(ex.Message);
                        editProfile.IsEnabled = true;
                        MainTabControl.IsEnabled = true;
                        save.Visibility = Visibility.Hidden;
                        AccountListData.IsEnabled = true;
                    }

                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                editProfile.IsEnabled = true;
                MainTabControl.IsEnabled = true;
                save.Visibility = Visibility.Hidden;
                AccountListData.IsEnabled = true;
            }
        }

        private void Cbb_class_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                //if ((cbb_class.SelectedItem as ComboBoxItem).Content.ToString() != "Student")
                //{
                //    tb_student.Text = "";
                //    tb_student.IsEnabled = false;
                //}
                //else
                //{
                //    tb_student.IsEnabled = true;
                //}
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

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            mainModel.ExportAllProfile();
        }

        private void Cbb_status_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Btn_changeStatus_Click(object sender, RoutedEventArgs e)
        {
            ProfileRF profileRF = (sender as System.Windows.Controls.Button).DataContext as ProfileRF;
            if(profileRF.STATUS == "Active")
            {
                profileRF.STATUS = "Suspended";
                profileRF.LOCK_DATE = DateTime.Now;
            }
            else
            {
                if (profileRF.STATUS == "Suspended")
                {
                    profileRF.STATUS = "Active";
                    profileRF.LOCK_DATE = DateTime.MinValue;
                }
            }
            SqliteDataAccess.UpdateProfileRF(profileRF, profileRF.STATUS);
            mainModel.ReloadListProfileRFDGV();
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            mainModel.ReloadListProfileRFDGV(tb_nameSearch.Text,tb_pinSearch.Text,tb_adnoSearch.Text);
        }

        private void Tb_Search_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Filter_Click(sender, e);
            }
        }

        private void Btn_exportTimeCheck_Click(object sender, RoutedEventArgs e)
        {
            mainModel.ExportListTimeCheck();
        }

        private void Btn_sync_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeviceRF deviceRF = (sender as System.Windows.Controls.Button).DataContext as DeviceRF;
                deviceRF.deviceItem.sendProfile(deviceRF.IP);
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        private void EditDevice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DeviceRFListData.SelectedItem != null)
                {
                    DeviceRF deviceRF = DeviceRFListData.SelectedItem as DeviceRF;
                    AddDeviceRFForm frm = new AddDeviceRFForm(this, deviceRF);
                    frm.ShowDialog();
                    mainModel.ReloadListDeviceRFDGV();
                }
                
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

            foreach (DeviceRF item in mainModel.deviceRFList)
            {
                if(item.deviceItem!=null)
                    item.deviceItem.Dispose();
            }
            Environment.Exit(0);
        }

        private void ChangeProfileStatus_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListData.SelectedItem != null)
            {
                ProfileRF profileRF = AccountListData.SelectedItem as ProfileRF;
                if (profileRF.STATUS == "Active")
                {
                    profileRF.STATUS = "Suspended";
                    profileRF.LOCK_DATE = DateTime.Now;
                }
                else
                {
                    profileRF.STATUS = "Active";
                    profileRF.LOCK_DATE = DateTime.MinValue;
                }
                SqliteDataAccess.UpdateProfileRF(profileRF, profileRF.STATUS);
                mainModel.ReloadListProfileRFDGV();
            }
        }
    }
}
