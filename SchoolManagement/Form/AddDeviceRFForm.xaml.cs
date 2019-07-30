using SchoolManagement.DTO;
using System;
using System.Windows;
using System.Windows.Forms;

namespace SchoolManagement.Form
{
    /// <summary>
    /// Interaction logic for AddDeviceRFForm.xaml
    /// </summary>
    public partial class AddDeviceRFForm : Window
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private MainWindow mainW;

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
                if (item.Equals("Staff")) { cb_staff.IsChecked = true; }
                if (item.Equals("Parent")) { cb_parent.IsChecked = true; }
                if (item.Equals("Student")) { cb_student.IsChecked = true; }
                if (item.Equals("Visitor")) { cb_visitor.IsChecked = true; }
                if (item.Equals("Long Term Supplier")) { cb_longTermSupplier.IsChecked = true; }
                if (item.Equals("Short Term Supplier")) { cb_shortTermSupplier.IsChecked = true; }
                if (item.Equals("Security")) { cb_security.IsChecked = true; }
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
                if ((!(bool)cb_staff.IsChecked) &&
                    (!(bool)cb_parent.IsChecked) &&
                    (!(bool)cb_student.IsChecked) &&
                    (!(bool)cb_visitor.IsChecked) &&
                    (!(bool)cb_longTermSupplier.IsChecked) &&
                    (!(bool)cb_shortTermSupplier.IsChecked) &&
                    (!(bool)cb_security.IsChecked))
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
                    ((bool)cb_staff.IsChecked ? "Staff" : "") +
                    ((bool)cb_parent.IsChecked ? ",Parent" : "") +
                    ((bool)cb_student.IsChecked ? ",Student" : "") +
                    ((bool)cb_visitor.IsChecked ? ",Visitor" : "") +
                    ((bool)cb_longTermSupplier.IsChecked ? ",Long Term Supplier" : "") +
                    ((bool)cb_shortTermSupplier.IsChecked ? ",Short Term Supplier" : "") +
                    ((bool)cb_security.IsChecked ? ",Security" : "");

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
            cb_staff.IsChecked =
                cb_parent.IsChecked =
                cb_student.IsChecked =
                cb_visitor.IsChecked =
                cb_longTermSupplier.IsChecked =
                cb_shortTermSupplier.IsChecked =
                cb_security.IsChecked = false;
        }
    }
}