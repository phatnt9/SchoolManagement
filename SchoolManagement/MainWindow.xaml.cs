using SchoolManagement.Form;
using System.Windows;
using SchoolManagement.Model;
using System.ComponentModel;

namespace SchoolManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowModel mainModel;
        public BackgroundWorker worker;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            mainModel = new MainWindowModel(this);
            DataContext = mainModel;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            mainModel.LoadDataFromExcel();
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


    }
}
