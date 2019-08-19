using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace SchoolManagement.View
{
    /// <summary>
    /// Interaction logic for SettingForm.xaml
    /// </summary>
    public partial class SettingForm : Window
    {
        private MainWindow mainW;
        private static readonly Regex _regex = new Regex("[^0-9.-]+");
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SettingForm(MainWindow mainW)
        {
            InitializeComponent();
            this.mainW = mainW;
            Loaded += SettingForm_Loaded;
        }

        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void Tb_requestProfileInterval_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                e.Handled = !IsTextAllowed(e.Text);
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }

        private void SettingForm_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                tb_requestProfileInterval.Text = (Properties.Settings.Default.RequestTimeCheckInterval / (double)60000).ToString();
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
                float time = float.Parse(tb_requestProfileInterval.Text) * 60000;
                Properties.Settings.Default.RequestTimeCheckInterval = (int)time;
                Properties.Settings.Default.Save();
                mainW.mainModel.timerSyncTimeSheet.Interval = Properties.Settings.Default.RequestTimeCheckInterval;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }
    }
}