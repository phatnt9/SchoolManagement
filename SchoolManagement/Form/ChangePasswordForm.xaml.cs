using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SchoolManagement.Form
{
    /// <summary>
    /// Interaction logic for ChangePasswordForm.xaml
    /// </summary>
    public partial class ChangePasswordForm : Window
    {
        MainWindow mainW;

        public ChangePasswordForm(MainWindow mainW)
        {
            InitializeComponent();
            this.mainW = mainW;
            Loaded += ChangePasswordForm_Loaded;
        }

        private void ChangePasswordForm_Loaded(object sender, RoutedEventArgs e)
        {
            userNametb.Text = "admin";
        }

        private void PasswordCurrenttb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.passwordNewtb.SelectAll();
                this.passwordNewtb.Focus();
            }
        }

        private void PasswordNewtb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.passwordNewConfirmtb.SelectAll();
                this.passwordNewConfirmtb.Focus();
            }
        }

        private void PasswordNewConfirmtb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btn_save_Click(sender, e);
            }
        }
        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            if (passwordCurrenttb.Password.Equals(Properties.Settings.Default.RootPassword.ToString()))
            {
                if (passwordNewtb.Password.Equals(passwordNewConfirmtb.Password))
                {
                    Properties.Settings.Default.RootPassword = passwordNewtb.Password;
                    Properties.Settings.Default.Save();
                    mainW.ChangeToLoginScreen();
                    this.Close();
                }
            }
        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
