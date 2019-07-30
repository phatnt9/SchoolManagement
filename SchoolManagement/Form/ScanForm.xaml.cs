using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace SchoolManagement.Form
{
    /// <summary>
    /// Interaction logic for ScanForm.xaml
    /// </summary>
    public partial class ScanForm : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Regex _regex = new Regex("[^0-9]");

        private RegisterForm registerForm;

        public ScanForm(RegisterForm registerForm)
        {
            InitializeComponent();
            this.registerForm = registerForm;
            Loaded += ScanForm_Loaded;
        }

        private void ScanForm_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
            tb_pinno.Focus();
        }

        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void Tb_pinno_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void Tb_pinno_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                registerForm.tb_serialId.Clear();
                registerForm.tb_serialId.Text = this.tb_pinno.Text.Trim();
                Close();
            }
        }
    }
}