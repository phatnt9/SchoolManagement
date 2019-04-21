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
            tb_comport.Text = Properties.Settings.Default.ComPortName.ToString();
        }

        private void Btn_scanId_Click(object sender, RoutedEventArgs e)
        {
            btn_scanId.IsEnabled = false;
            btn_scanId.Content = "Waitting";
            lb_status.Content = "Initializing";

            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
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
                System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "tb_serialId", "tb_serialId"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            if (String.IsNullOrEmpty(tb_studentName.Text.ToString()) || tb_studentName.Text.ToString().Trim() == "")
            {
                System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "tb_studentName", "tb_studentName"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.tb_studentName.Focus();
                return;
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
            
            

            CreateNewPerson();
        }

        private void CreateNewPerson()
        {
            ProfileRF person = new ProfileRF();
            person.SERIAL_ID = tb_serialId.Text;
            person.NAME = tb_name.Text;
            person.GENDER = ((bool)rb_male.IsChecked) ? Constant.Gender.Male : Constant.Gender.Female;
            person.CLASS = (Constant.AccountClass)cbb_class.SelectedIndex;
            person.BIRTHDAY = (DateTime)dp_dateofbirth.SelectedDate;
            person.STUDENT = tb_studentName.Text;
            person.EMAIL = tb_email.Text;
            person.ADDRESS = tb_address.Text;
            person.PHONE = tb_phone.Text;
            try
            {

                SqliteDataAccess.SaveProfileRF(person);
                lb_status.Content = "New Profile Added";
                ClearForm();
            }
            catch (Exception ex)
            {
                lb_status.Content = "Error add new person";
                logFile.Error(ex.Message);
            }
        }

        public void ClearForm()
        {
            tb_serialId.Clear();
            tb_name.Clear();
            rb_male.IsChecked = true;
            tb_studentName.Clear();
            tb_email.Clear();
            tb_address.Clear();
            tb_phone.Clear();
        }

        private void Btn_edit_Click(object sender, RoutedEventArgs e)
        {
            if (!tb_comport.IsEnabled)
            {
                tb_comport.IsEnabled = true;
                btn_scanId.IsEnabled = false;
                btn_edit.Content = "Save";
            }
            else
            {
                if (String.IsNullOrEmpty(tb_comport.Text.ToString()) || tb_comport.Text.ToString().Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "tb_comport", "tb_comport"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tb_comport.Focus();
                    return;
                }
                Properties.Settings.Default.ComPortName = tb_comport.Text;
                Properties.Settings.Default.Save();
                tb_comport.Text = Properties.Settings.Default.ComPortName.ToString();
                tb_comport.IsEnabled = false;
                btn_scanId.IsEnabled = true;
                btn_edit.Content = "Edit";
            }
        }
    }
}
