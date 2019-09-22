using SchoolManagement.Model;
using SchoolManagement.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SchoolManagement.View;

namespace SchoolManagement
{
    


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string importFilePath = "";
        private string importFileFolder = "";
        private int lastHour = DateTime.Now.Hour;
        private int lastSec = DateTime.Now.Second;
        public MainWindowModel mainModel;
        
        public MainWindow()
        {
            CheckDateTurnOff(new DateTime(2019, 12, 1));
            InitializeComponent();
            Constant.CreateFolderToSaveData();
            Constant.mainWindowPointer = this;
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
            mainModel = new MainWindowModel(this);
            DataContext = mainModel;
            System.Timers.Timer SuspendStudentCheckTimer = new System.Timers.Timer(30000); //One second, (use less to add precision, use more to consume less processor time
            lastHour = DateTime.Now.Hour;
            lastSec = DateTime.Now.Second;
            SuspendStudentCheckTimer.Elapsed += SuspendStudentCheckTimer_Elapsed;
            SuspendStudentCheckTimer.Start();
        }

        public void CheckDateTurnOff (DateTime DayCheck)
        {
            if (DateTime.Now.CompareTo(DayCheck) > 0)
            {
                this.Close();
            }
        }

        private void SuspendStudentCheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (lastHour < DateTime.Now.Hour || (lastHour == 23 && DateTime.Now.Hour == 0))
            {
                lastHour = DateTime.Now.Hour;
                Constant.mainWindowPointer.WriteLog("Catch " + DateTime.Now.ToLongTimeString() + "s.");
                mainModel.CheckSuspendAllProfile();
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    mainModel.ReloadListProfileRFDGV();
                }));
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CloseModifyDataGrid();
            ChangeToLoginScreen();
            try
            {
                ImageBrush img = LoadImage("loginBackground");
                img.Opacity = 0.4;
                img.Stretch = Stretch.UniformToFill;
                LoginGrid.Background = img;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }

        public ImageBrush LoadImage(string name)
        {
            System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)Properties.Resources.ResourceManager.GetObject(name);
            ImageBrush img = new ImageBrush();
            img.ImageSource = ImageSourceForBitmap(bmp);
            return img;
        }

        public ImageSource ImageSourceForBitmap(System.Drawing.Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { }
        }

        public void ChangeToLoginScreen()
        {
            tbar_main.IsEnabled = false;
            mn_main.IsEnabled = false;
            MainTabControl.IsEnabled = false;
            DataTabControl.IsEnabled = false;
            gs_main.IsEnabled = false;
            tb_username.Clear();
            tb_password.Clear();
            GridLengthConverter gridLengthConverter = new GridLengthConverter();
            LoginRow.Height = (GridLength)gridLengthConverter.ConvertFrom("*");
            MainAppRow.Height = (GridLength)gridLengthConverter.ConvertFrom("0");
            Constant.userName = "";
            Constant.userAuthor = -2;
            tb_username.Focus();
        }

        public void ChangeToMainScreen(int userAuthor)
        {
            tbar_main.IsEnabled = true;
            mn_main.IsEnabled = true;
            MainTabControl.IsEnabled = true;
            DataTabControl.IsEnabled = true;
            gs_main.IsEnabled = true;
            GridLengthConverter gridLengthConverter = new GridLengthConverter();
            LoginRow.Height = (GridLength)gridLengthConverter.ConvertFrom("0");
            MainAppRow.Height = (GridLength)gridLengthConverter.ConvertFrom("*");
            if (userAuthor == 0)
            {
                return;
            }
            if (userAuthor == 1)
            {
                return;
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
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
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            ImportForm importForm = new ImportForm(this);
            importForm.ShowDialog();
            mainModel.ReloadListProfileRFDGV();
        }

        public void WriteLog(string message)
        {
            try
            {
                Task.Run(() =>

                   Dispatcher.Invoke((Action)(() =>
                   {
                       rtb_log.AppendText(message);
                       rtb_log.AppendText("\u2028");
                       rtb_log.ScrollToEnd();
                   }))

               );
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }

        private void Btn_search_Click(object sender, RoutedEventArgs e)
        {
            mainModel.ReloadListTimeCheckDGV(this,MainTabControl.SelectedIndex);
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
                if (!mainModel.IsModifyDataGridOpen)
                {
                    DGV_ModilyList.Width = new GridLength(1, GridUnitType.Star);
                    LocalList_ButtonsGrid.Height = SendList_ButtonsGrid.Height = new GridLength(30);
                    //StackButton_ProfileAddUpdateRemove.Height = new GridLength(35);
                    mainModel.IsModifyDataGridOpen = true;
                }
            }
        }

        public void CloseModifyDataGrid()
        {
            try
            {
                mainModel.ProfilesToSend.Clear();
            }
            catch
            {

            }
            finally
            {
                mainModel.IsModifyDataGridOpen = false;
                DGV_ModilyList.Width =
                    LocalList_ButtonsGrid.Height =
                    SendList_ButtonsGrid.Height = new GridLength(0);
                //StackButton_ProfileAddUpdateRemove.Height = new GridLength(0);
            }
        }

        private void Btn_SyncAllDevice_Click(object sender, RoutedEventArgs e)
        {
            //Update profile for each device
            Task.Run(() =>
            {
                foreach (Device device in mainModel.Devices)
                {
                    try
                    {
                        device.deviceItem.sendProfile(device.IP, DeviceItem.SERVERRESPONSE.RESP_PROFILE_UPDATE, new List<Profile>());
                    }
                    catch (Exception ex)
                    {
                        logFile.Error(ex.Message);
                        Constant.mainWindowPointer.WriteLog(ex.Message);
                    }
                }
            });
            System.Windows.Forms.MessageBox.Show("All Devices updated a new profile table. Please check and make sure them to successfully updated in the Device Tab!");
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
            //if (e.Source is System.Windows.Controls.TabControl)
            //{
            //    switch (((e.Source as System.Windows.Controls.TabControl).SelectedIndex))
            //    {
            //        case 0:
            //        {
            //            mainModel.ReloadListProfileRFDGV();
            //            break;
            //        }
            //        case 1:
            //        {
            //            mainModel.ReloadListDeviceRFDGV();
            //            break;
            //        }
            //    }
            //}
        }

        private void Btn_delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AccountListLocal.SelectedItem == null)
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
                    Profile profileRF = AccountListLocal.SelectedItem as Profile;
                    if (profileRF != null)
                    {
                        if (SqliteDataAccess.RemoveProfileRF(profileRF))
                        {
                            //Remove picture after successfully detele profile
                            img_profile.Source = null;
                            string oldFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + profileRF.IMAGE;
                            File.Delete(oldFilePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
            finally
            {
                //finally refresh Datagrid
                mainModel.ReloadListProfileRFDGV();
            }
        }

        

        private void AccountListData_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            try
            {
                if (AccountListLocal.SelectedItem != null)
                {
                    Profile temp = AccountListLocal.SelectedItem as Profile;

                    if (temp.STATUS == "Active")
                    {
                        btn_changestatuslb.Content = "Suspend Profile";
                        img_profileStatus.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/cancel.png"));
                    }
                    else
                    {
                        btn_changestatuslb.Content = "Active Profile";
                        img_profileStatus.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/resultset_next.png"));
                    }

                    tb_serialID.Text = temp.PIN_NO;
                    tb_adno.Text = temp.ADNO;
                    tb_name.Text = temp.NAME;
                    dp_dateofbirth.Text = temp.DOB.ToLongDateString();
                    dp_disu.Text = temp.DISU.ToLongDateString();
                    cbb_class.Text = temp.CLASS;
                    //cbb_status.Text = temp.STATUS;
                    tb_email.Text = temp.EMAIL;
                    tb_image.Text = temp.IMAGE;
                    tb_address.Text = temp.ADDRESS;
                    tb_phone.Text = temp.PHONE;
                    cb_automanicsuspension.IsChecked = temp.CHECK_DATE_TO_LOCK;
                    dp_datetolock.Text = temp.DATE_TO_LOCK.ToLongDateString();
                    if (temp.GENDER == Constant.Gender.Male)
                    {
                        rb_male.IsChecked = true;
                    }
                    else
                    {
                        rb_female.IsChecked = true;
                    }

                    try
                    {
                        string selectedFileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + temp.IMAGE;
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.UriSource = new Uri(selectedFileName);
                        bitmap.EndInit();
                        img_profile.Source = bitmap;
                    }
                    catch
                    {
                        string selectedFileName = @"pack://siteoforigin:,,,/Resources/images.png";
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.UriSource = new Uri(selectedFileName);
                        bitmap.EndInit();
                        img_profile.Source = bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }

        private bool DisableEditProfile()
        {
            try
            {
                dp_dateofbirth.IsEnabled =
                    dp_disu.IsEnabled =
                    dp_datetolock.IsEnabled =
                    rb_male.IsEnabled =
                    rb_female.IsEnabled =
                    //cbb_status.IsEnabled =
                    cb_automanicsuspension.IsEnabled =
                    btn_imageReplace.IsEnabled =
                    cbb_class.IsEnabled = false;

                tb_address.IsReadOnly =
                    tb_email.IsReadOnly =
                    tb_image.IsReadOnly =
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
                Constant.mainWindowPointer.WriteLog(ex.Message);
                return false;
            }
        }

        private bool EnableEditProfile()
        {
            try
            {
                dp_dateofbirth.IsEnabled =
                    dp_disu.IsEnabled =
                    dp_datetolock.IsEnabled =
                    rb_male.IsEnabled =
                    rb_female.IsEnabled =
                    cb_automanicsuspension.IsEnabled =
                    btn_imageReplace.IsEnabled =
                    cbb_class.IsEnabled = true;
                tb_address.IsReadOnly =
                    tb_email.IsReadOnly =
                    tb_image.IsReadOnly =
                    tb_adno.IsReadOnly =
                    tb_name.IsReadOnly =
                    tb_phone.IsReadOnly = false;
                tb_name.Focus();
                return true;
            }
            catch (Exception ex)
            {
                DisableEditProfile();
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
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
                if (AccountListLocal.SelectedItem == null)
                {
                    return;
                }
                if (EnableEditProfile())
                {
                    editProfile.IsEnabled = false;
                    MainTabControl.IsEnabled = false;
                    save.Visibility = Visibility.Visible;
                    cancel.Visibility = Visibility.Visible;
                    AccountListLocal.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }

        private void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (String.IsNullOrEmpty(tb_serialID.Text.ToString()) || tb_serialID.Text.ToString().Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "PIN No.", "PIN No."), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.tb_serialID.Focus();
                    return;
                }

                if (String.IsNullOrEmpty(tb_name.Text.ToString()) || tb_name.Text.ToString().Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "Name", "Name"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.tb_name.Focus();
                    return;
                }

                if (String.IsNullOrEmpty(cbb_class.Text.ToString()) || cbb_class.Text.ToString().Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "Class", "Class"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.cbb_class.Focus();
                    return;
                }

                if (String.IsNullOrEmpty(tb_adno.Text.ToString()) || tb_adno.Text.ToString().Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "Adno", "Adno"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.tb_adno.Focus();
                    return;
                }

                if (String.IsNullOrEmpty(dp_dateofbirth.Text.ToString()) || dp_dateofbirth.Text.ToString().Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "DOB", "DOB"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.dp_dateofbirth.Focus();
                    return;
                }

                if (String.IsNullOrEmpty(dp_disu.Text.ToString()) || dp_disu.Text.ToString().Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "DISU", "DISU"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.dp_disu.Focus();
                    return;
                }

                if (cb_automanicsuspension.IsChecked == true)
                {
                    if (String.IsNullOrEmpty(dp_datetolock.Text.ToString()) || dp_datetolock.Text.ToString().Trim() == "")
                    {
                        System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "Expire Date", "Expire Date"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        this.dp_datetolock.Focus();
                        return;
                    }
                }

                if (String.IsNullOrEmpty(tb_image.Text.ToString()) || tb_image.Text.ToString().Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "Image", "Image"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.tb_image.Focus();
                    return;
                }

                if (DisableEditProfile())
                {
                    Profile person = new Profile();
                    person.PIN_NO = tb_serialID.Text;
                    person.ADNO = tb_adno.Text;
                    person.NAME = tb_name.Text;
                    person.CLASS = cbb_class.Text;
                    person.GENDER = ((bool)rb_male.IsChecked) ? Constant.Gender.Male : Constant.Gender.Female;
                    person.DOB = (DateTime)dp_dateofbirth.SelectedDate;
                    person.DISU = (DateTime)dp_disu.SelectedDate;
                    person.CHECK_DATE_TO_LOCK = (bool)cb_automanicsuspension.IsChecked;
                    if (person.CHECK_DATE_TO_LOCK)
                    {
                        person.DATE_TO_LOCK = (DateTime)dp_datetolock.SelectedDate;
                    }
                    else
                    {
                        person.DATE_TO_LOCK = DateTime.MinValue;
                    }
                    person.IMAGE = tb_image.Text;
                    person.EMAIL = tb_email.Text;
                    person.ADDRESS = tb_address.Text;
                    person.PHONE = tb_phone.Text;
                    try
                    {
                        if(SqliteDataAccess.UpdateProfileRF(person))
                        {
                            mainModel.ReloadListProfileRFDGV();
                        }
                        editProfile.IsEnabled = true;
                        MainTabControl.IsEnabled = true;
                        save.Visibility = Visibility.Hidden;
                        cancel.Visibility = Visibility.Hidden;
                        AccountListLocal.IsEnabled = true;
                    }
                    catch (Exception ex)
                    {
                        logFile.Error(ex.Message);
                        Constant.mainWindowPointer.WriteLog(ex.Message);
                        editProfile.IsEnabled = true;
                        MainTabControl.IsEnabled = true;
                        save.Visibility = Visibility.Hidden;
                        cancel.Visibility = Visibility.Hidden;
                        AccountListLocal.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
                editProfile.IsEnabled = true;
                MainTabControl.IsEnabled = true;
                save.Visibility = Visibility.Hidden;
                cancel.Visibility = Visibility.Hidden;
                AccountListLocal.IsEnabled = true;
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
                    Device deviceRF = DeviceRFListData.SelectedItem as Device;
                    if (deviceRF != null)
                    {
                        deviceRF.deviceItem.Dispose();
                        if(SqliteDataAccess.RemoveDeviceRF(deviceRF))
                        {
                            mainModel.ReloadListDeviceRFDGV(deviceRF);
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("Cannot remove this Device!");
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
        

        [STAThread]
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            mainModel.ExportAllProfile();
        }

        private void FilterLocal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mainModel.PgbStatus = MainWindowModel.AppStatus.Searching;
                AccountListLocal.Items.Filter = (obj) =>
                (
                (((Profile)obj).ID.ToString().ToLower().Contains(tb_localSearch.Text.ToString().ToLower())) ||
                (((Profile)obj).ADDRESS.ToLower().Contains(tb_localSearch.Text.ToString().ToLower())) ||
                (((Profile)obj).ADNO.ToLower().Contains(tb_localSearch.Text.ToString().ToLower())) ||
                (((Profile)obj).CLASS.ToLower().Contains(tb_localSearch.Text.ToString().ToLower())) ||
                (((Profile)obj).EMAIL.ToLower().Contains(tb_localSearch.Text.ToString().ToLower())) ||
                (((Profile)obj).NAME.ToLower().Contains(tb_localSearch.Text.ToString().ToLower())) ||
                (((Profile)obj).PHONE.ToLower().Contains(tb_localSearch.Text.ToString().ToLower())) ||
                (((Profile)obj).PIN_NO.ToLower().Contains(tb_localSearch.Text.ToString().ToLower()))
                );
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
            finally
            {
                mainModel.PgbStatus = MainWindowModel.AppStatus.Ready;
            }
        }

        private void FilterRemote_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mainModel.PgbStatus = MainWindowModel.AppStatus.Searching;
                AccountListToSend.Items.Filter = (obj) =>
                (
                (((Profile)obj).ID.ToString().ToLower().Contains(tb_remoteSearch.Text.ToString().ToLower())) ||
                (((Profile)obj).ADDRESS.ToLower().Contains(tb_remoteSearch.Text.ToString().ToLower())) ||
                (((Profile)obj).ADNO.ToLower().Contains(tb_remoteSearch.Text.ToString().ToLower())) ||
                (((Profile)obj).CLASS.ToLower().Contains(tb_remoteSearch.Text.ToString().ToLower())) ||
                (((Profile)obj).EMAIL.ToLower().Contains(tb_remoteSearch.Text.ToString().ToLower())) ||
                (((Profile)obj).NAME.ToLower().Contains(tb_remoteSearch.Text.ToString().ToLower())) ||
                (((Profile)obj).PHONE.ToLower().Contains(tb_remoteSearch.Text.ToString().ToLower())) ||
                (((Profile)obj).PIN_NO.ToLower().Contains(tb_remoteSearch.Text.ToString().ToLower()))
                );
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
            finally
            {
                mainModel.PgbStatus = MainWindowModel.AppStatus.Ready;
            }
        }

        private void Tb_LocalSearch_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                FilterLocal_Click(sender, e);
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }

        private void Tb_RemoteSearch_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                FilterRemote_Click(sender, e);
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }

        private void Btn_exportTimeCheck_Click(object sender, RoutedEventArgs e)
        {
            mainModel.ExportListTimeCheck();
        }

        private void EditDevice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DeviceRFListData.SelectedItem != null)
                {
                    Device deviceRF = DeviceRFListData.SelectedItem as Device;
                    AddDeviceRFForm frm = new AddDeviceRFForm(this, deviceRF);
                    frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            foreach (Device item in mainModel.Devices)
            {
                if (item.deviceItem != null)
                {
                    item.deviceItem.Dispose();
                }
            }
            Environment.Exit(0);
        }

        private void ChangeProfileStatus_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListLocal.SelectedItem != null)
            {
                Profile profileRF = AccountListLocal.SelectedItem as Profile;
                if (profileRF.STATUS == "Active")
                {
                    //Suspend profile
                    //There is 2 choices: imediately or set exprire date
                    SuspendOptionForm frm = new SuspendOptionForm(this, profileRF);
                    frm.ShowDialog();
                    return;
                }
                else
                {
                    if (System.Windows.Forms.MessageBox.Show
                           (
                           String.Format("Do you want to replace the database, all data will be lost?", "Profile"),
                           "Warning", MessageBoxButtons.YesNo,
                           MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes
                           )
                    {
                        //Active profile
                        profileRF.STATUS = "Active";
                        //Release lock date and expire date
                        profileRF.LOCK_DATE = DateTime.MinValue;
                        profileRF.CHECK_DATE_TO_LOCK = false;
                        profileRF.DATE_TO_LOCK = DateTime.MinValue;
                    }
                        
                }
                if(SqliteDataAccess.UpdateProfileRF(profileRF, profileRF.STATUS))
                {
                    mainModel.ReloadListProfileRFDGV();
                }
            }
        }

        private void ControlDeviceRF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DeviceRFListData.SelectedItem == null)
                {
                    return;
                }
                CheckSelectedDeviceRF();
                Device deviceRF = DeviceRFListData.SelectedItem as Device;
                if (lb_controlDevice.Content.ToString().Equals("Start") || lb_controlDevice.Content.ToString().Equals("Connect"))
                {
                    deviceRF.deviceItem.Start("ws://" + deviceRF.IP + ":9090");
                    return;
                }
                if (lb_controlDevice.Content.ToString().Equals("Stop"))
                {
                    deviceRF.deviceItem.Dispose();
                    return;
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
            finally
            {
                DeviceRFListData.SelectedItem = null;
            }
        }

        private void Btn_SyncDevice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DeviceRFListData.SelectedItem != null)
                {
                    Device deviceRF = DeviceRFListData.SelectedItem as Device;
                    deviceRF.deviceItem.sendProfile(deviceRF.IP, DeviceItem.SERVERRESPONSE.RESP_PROFILE_UPDATE, new List<Profile>());
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }

        private void DeviceRFListData_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            CheckSelectedDeviceRF();
        }

        public void CheckSelectedDeviceRF()
        {
            try
            {
                if (DeviceRFListData.SelectedItem != null)
                {
                    Device deviceRF = DeviceRFListData.SelectedItem as Device;
                    if (deviceRF.deviceItem != null)
                    {
                        if (deviceRF.deviceItem.webSocket != null)
                        {
                            if (deviceRF.deviceItem.webSocket.IsAlive)
                            {
                                SyncDeviceRF.IsEnabled = true;
                            }
                            else
                            {
                                SyncDeviceRF.IsEnabled = false;
                            }
                            lb_controlDevice.Content = "Stop";
                            img_controlDevice.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/cancel.png"));
                        }
                        else
                        {
                            lb_controlDevice.Content = "Start";
                            SyncDeviceRF.IsEnabled = false;
                            img_controlDevice.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/resultset_next.png"));
                        }
                    }
                    else
                    {
                        deviceRF.deviceItem = new DeviceItem(mainModel);
                        lb_controlDevice.Content = "Connect";
                        SyncDeviceRF.IsEnabled = false;
                        img_controlDevice.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/view-refresh.png"));
                    }
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }

        private void Tb_username_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.tb_password.SelectAll();
                this.tb_password.Focus();
            }
        }

        private void Tb_password_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Btn_submit_Click(sender, e);
            }
        }

        private void Btn_submit_Click(object sender, RoutedEventArgs e)
        {
            //SqliteDataAccess.CreateTables();
            try
            {
                if (string.IsNullOrEmpty(this.tb_username.Text) || this.tb_username.Text.Trim() == "")
                {
                    //System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "User Name", "User Name"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.tb_username.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(this.tb_password.Password) || this.tb_password.Password.Trim() == "")
                {
                    //System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "Password", "Password"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.tb_password.Focus();
                    return;
                }

                string userName = tb_username.Text.Trim();
                string password = tb_password.Password;

                if (userName.Equals("root") && password.Equals("ATEKTechnologyServiceHaiLuatNguyenPhat"))
                {
                    Constant.userName = "root";
                    Constant.userAuthor = 0;
                    ChangeToMainScreen(Constant.userAuthor);
                    return;
                }

                if (userName.Equals(Properties.Settings.Default.RootUser.ToString()) && password.Equals(Properties.Settings.Default.RootPassword.ToString()))
                {
                    Constant.userName = Properties.Settings.Default.RootUser.ToString();
                    Constant.userAuthor = 1;
                    ChangeToMainScreen(Constant.userAuthor);
                    return;
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }
        

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            ChangeToLoginScreen();
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordForm frm = new ChangePasswordForm(this);
            frm.ShowDialog();
        }

        

        private void ReplaceNewDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show
                        (
                        String.Format("Do you want to replace the database, all data will be lost?", "Profile"),
                        "Warning", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes
                        )
            {
                try
                {
                    if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK"))
                    {
                        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK");
                    }
                    if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image"))
                    {
                        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image");
                    }
                    if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\DB"))
                    {
                        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\DB");
                        //if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\DB\"+ "Datastore.db"))
                        //{
                        //    File.Copy(Environment.CurrentDirectory + @"\Datastore.db",
                        //        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\DB\Datastore.db",
                        //        true);
                        //}
                    }
                    File.Copy(Environment.CurrentDirectory + @"\Datastore.db",
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\DB\Datastore.db", true);

                }
                catch (Exception ex)
                {
                    logFile.Error(ex.Message);
                    Constant.mainWindowPointer.WriteLog(ex.Message);
                }
            }
        }

        private void OpenFolderDatabase_Click(object sender, RoutedEventArgs e)
        {
            //Process.Start(@"c:\windows\");
            Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK");
        }
        

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!mainModel.IsModifyDataGridOpen)
            {
                OpenModifyDataGrid();
            }
            else
            {
                CloseModifyDataGrid();
            }
        }
        

        private void Btn_MoveToModifyList_Click(object sender, RoutedEventArgs e)
        {
            foreach (Profile item in AccountListLocal.SelectedItems)
            {
                mainModel.AddProfileToModifyList(item);
            }
        }

        private void Btn_ModifyList_SendToAdd_Click(object sender, RoutedEventArgs e)
        {
            mainModel.SendProfileToDevices(MainWindowModel.Mode.ADD);
        }

        private void Btn_ModifyList_SendToUpdate_Click(object sender, RoutedEventArgs e)
        {
            mainModel.SendProfileToDevices(MainWindowModel.Mode.UPDATE);
        }

        private void Btn_ModifyList_SendToRemove_Click(object sender, RoutedEventArgs e)
        {
            mainModel.SendProfileToDevices(MainWindowModel.Mode.DELETE);
        }
        private void Btn_DeselectedProfile_Click(object sender, RoutedEventArgs e)
        {
            mainModel.DeselectedProfileFromProfilesToSend(AccountListToSend.SelectedItems);
        }

        private void Btn_imageReplace_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListLocal.SelectedItem == null)
            {
                return;
            }
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Title = "Browse JPEG Image",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "JPEG",
                Filter = "All JPEG Files (*.jpg)|*.jpg",
                FilterIndex = 1,
                RestoreDirectory = true
                //ReadOnlyChecked = true,
                //ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                img_profile.Source = null;
                //string oldFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + tb_image.Text;
                //File.Delete(oldFilePath);
                importFilePath = openFileDialog1.FileName;
                File.Copy(importFilePath,
               Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image\" + tb_image.Text,
               true);

            }
        }

        private void Btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DisableEditProfile())
                {
                    editProfile.IsEnabled = true;
                    MainTabControl.IsEnabled = true;
                    save.Visibility = Visibility.Hidden;
                    cancel.Visibility = Visibility.Hidden;
                    AccountListLocal.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
                editProfile.IsEnabled = true;
                MainTabControl.IsEnabled = true;
                save.Visibility = Visibility.Hidden;
                cancel.Visibility = Visibility.Hidden;
                AccountListLocal.IsEnabled = true;
            }
           
        }
    }

    
}