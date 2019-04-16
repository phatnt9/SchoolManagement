using SchoolManagement.Form;
using System.Windows;
using SchoolManagement.Model;
using System.ComponentModel;
using SchoolManagement.DTO;
using System;

namespace SchoolManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowModel mainModel;
        public BackgroundWorker workerProfile;
        public BackgroundWorker workerTimeCheck;
        public BackgroundWorker workerDatagridTimeCheck;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            mainModel = new MainWindowModel(this);
            DataContext = mainModel;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            mainModel.LoadProfileFromExcel();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Registration_Click(object sender, RoutedEventArgs e)
        {
            RegisterForm regForm = new RegisterForm();
            regForm.ShowDialog();
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            ImportForm importForm = new ImportForm();
            importForm.ShowDialog();

        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(DateTime.Now.Ticks);
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

        private void Btn_search_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListData.SelectedItem != null)
            {
                mainModel.LoadTimeCheck((AccountListData.SelectedItem as structExcel).serialId);
            }
        }
    }
}
