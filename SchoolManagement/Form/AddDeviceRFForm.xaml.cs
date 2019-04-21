using SchoolManagement.DTO;
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


        public AddDeviceRFForm()
        {
            InitializeComponent();
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
                if (String.IsNullOrEmpty(cbb_class.Text.ToString()) || cbb_class.Text.ToString().Trim() == "")
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "cbb_class", "cbb_class"), Constant.messageTitileWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.cbb_class.Focus();
                    return;
                }

                DeviceRF deviceRF = new DeviceRF();
                deviceRF.IP = tb_ip.Text;
                deviceRF.CLASS = (AccountClass)cbb_class.SelectedIndex;
                SqliteDataAccess.SaveDeviceRF(deviceRF);
                lb_status.Content = "New Device Added";
                ClearForm();
            }
            catch (Exception ex)
            {
                lb_status.Content = "Error add new device";
                logFile.Error(ex.Message);
            }
            
        }

        public void ClearForm()
        {
            tb_ip.Clear();
        }
    }
}
