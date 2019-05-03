using Newtonsoft.Json;
using SchoolManagement.Communication;
using SchoolManagement.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace SchoolManagement.Form
{
    /// <summary>
    /// Interaction logic for RegisterForm.xaml
    /// </summary>
    public partial class RegisterForm : Window
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public MainWindow mainWindow;

        private BackgroundWorker worker;
        private SerialCOM serial;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public RegisterForm(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            Loaded += RegisterForm_Loaded;
        }

        private void RegisterForm_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Btn_scanId_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btn_scanId.IsEnabled = false;
                //btn_scanId.Content = "Waitting";
                //lb_status.Content = "Initializing";

                ScanForm scanForm = new ScanForm(this);
                scanForm.ShowDialog();
                btn_scanId.IsEnabled = true;
                //worker = new BackgroundWorker();
                //worker.WorkerSupportsCancellation = true;
                //worker.DoWork += Worker_DoWork;
                //worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                //worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
            
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (serial == null)
            {
                serial = new SerialCOM(Properties.Settings.Default.ComPortName.ToString(), Properties.Settings.Default.ComPortBaudrate);
                serial.Open();
            }
            if (serial._serialPort.IsOpen)
            {
                Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    lb_status.Content = "Scanning...";
                }));
                e.Result = serial.ReceiveData();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // check error, check cancel, then use result
            if (e.Error != null)
            {
                // handle the error
                lb_status.Content = "Error";
            }
            else if (e.Cancelled)
            {
                // handle cancellation
                lb_status.Content = "Scanner not found";
            }
            else
            {
                // use it on the UI thread
                try
                {
                    string receivedDate = (string)e.Result;
                    if (receivedDate != "Null")
                    {
                        dynamic data = JsonConvert.DeserializeObject(receivedDate);
                        tb_serialId.Text = data.serialId;
                        lb_status.Content = "";
                    }
                    else
                    {
                        lb_status.Content = "Error";
                        tb_serialId.Text = "";
                    }
                }
                catch (Exception ex)
                {
                    logFile.Error(ex.Message);
                    Constant.mainWindowPointer.WriteLog(ex.Message);
                }

            }
            // general cleanup code, runs when there was an error or not.
            serial.Close();
            serial = null;
            btn_scanId.IsEnabled = true;
            btn_scanId.Content = "Scan";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(tb_serialId.Text.ToString()) || tb_serialId.Text.ToString().Trim() == "")
            {
                System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "PIN No.", "PIN No."), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.tb_serialId.Focus();
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

            if (String.IsNullOrEmpty(tb_image.Text.ToString()) || tb_image.Text.ToString().Trim() == "")
            {
                System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "Image", "Image"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.tb_image.Focus();
                return;
            }
            
            if (String.IsNullOrEmpty(tb_adno.Text.ToString()) || tb_adno.Text.ToString().Trim() == "")
            {
                System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "Adno", "Adno"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.tb_adno.Focus();
                return;
            }
            CreateNewPerson();
        }

        private void CreateNewPerson()
        {
            ProfileRF person = new ProfileRF();
            person.PIN_NO = tb_serialId.Text;
            person.NAME = tb_name.Text;
            person.CLASS = cbb_class.Text;
            person.GENDER = ((bool)rb_male.IsChecked) ? Constant.Gender.Male : Constant.Gender.Female;
            person.DOB = (DateTime)dp_dateofbirth.SelectedDate;
            //if (person.CLASS == "Student")
            //{
            //    person.STUDENT = tb_studentName.Text;
            //}
            //else
            //{
            //    person.STUDENT = "";
            //}
            person.EMAIL = tb_email.Text;
            person.ADDRESS = tb_address.Text;
            person.PHONE = tb_phone.Text;
            person.ADNO = tb_adno.Text;
            person.DISU = (DateTime)dp_disu.SelectedDate;
            person.IMAGE = tb_image.Text;
            person.STATUS = "Active";
            try
            {
                SqliteDataAccess.SaveProfileRF(person);
                lb_status.Content = "New Profile Added";
                ClearForm();
                mainWindow.mainModel.ReloadListProfileRFDGV();
            }
            catch (Exception ex)
            {
                lb_status.Content = "Error add new person";
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }

        public void ClearForm()
        {
            tb_serialId.Clear();
            tb_name.Clear();
            tb_adno.Clear();
            rb_male.IsChecked = true;
            //tb_studentName.Clear();
            tb_email.Clear();
            tb_address.Clear();
            tb_phone.Clear();
        }

        //private void Btn_edit_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!tb_comport.IsEnabled)
        //    {
        //        tb_comport.IsEnabled = true;
        //        btn_scanId.IsEnabled = false;
        //        btn_edit.Content = "Save";
        //    }
        //    else
        //    {
        //        if (String.IsNullOrEmpty(tb_comport.Text.ToString()) || tb_comport.Text.ToString().Trim() == "")
        //        {
        //            System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "tb_comport", "tb_comport"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            tb_comport.Focus();
        //            return;
        //        }
        //        Properties.Settings.Default.ComPortName = tb_comport.Text;
        //        Properties.Settings.Default.Save();
        //        tb_comport.Text = Properties.Settings.Default.ComPortName.ToString();
        //        tb_comport.IsEnabled = false;
        //        btn_scanId.IsEnabled = true;
        //        btn_edit.Content = "Edit";
        //    }
        //}

        private void Cbb_class_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //if ((cbb_class.SelectedItem as ComboBoxItem).Content.ToString() != "Student")
                //{
                //    tb_studentName.Text = "";
                //    tb_studentName.IsEnabled = false;
                //}
                //else
                //{
                //    tb_studentName.IsEnabled = true;
                //}
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }

        }
    }
}
