using SchoolManagement.Model;
using System;
using System.Windows;

namespace SchoolManagement.View
{
    /// <summary>
    /// Interaction logic for SuspendOptionForm.xaml
    /// </summary>
    public partial class SuspendOptionForm : Window
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private MainWindow mainW;
        private Profile profileRF;

        public SuspendOptionForm(MainWindow mainW, Profile profileRF)
        {
            this.mainW = mainW;
            this.profileRF = profileRF;
            InitializeComponent();
        }

        private void Rb_now_Checked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Rb_now_Checked");
        }

        private void Rb_later_Checked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Rb_later_Checked");
        }

        private void Btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Btn_accept_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (rb_now.IsChecked == true)
                {
                    profileRF.STATUS = "Suspended";
                    profileRF.LOCK_DATE = DateTime.Now;

                    profileRF.CHECK_DATE_TO_LOCK = false;
                    profileRF.DATE_TO_LOCK = DateTime.MinValue;

                    SqliteDataAccess.UpdateProfileRF(profileRF, profileRF.STATUS);
                    mainW.mainModel.ReloadListProfileRFDGV();
                    return;
                }
                if (rb_later.IsChecked == true)
                {
                    profileRF.CHECK_DATE_TO_LOCK = true;
                    profileRF.DATE_TO_LOCK = (DateTime)dp_laterOn.SelectedDate;

                    SqliteDataAccess.UpdateProfileRF(profileRF, profileRF.STATUS);
                    mainW.mainModel.ReloadListProfileRFDGV();
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
                Close();
            }
        }
    }
}