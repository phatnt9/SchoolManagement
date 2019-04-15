using System;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.Generic;
using SchoolManagement.DTO;
using System.ComponentModel;
using System.Threading;

namespace SchoolManagement.Form
{
    /// <summary>
    /// Interaction logic for ImportForm.xaml
    /// </summary>
    public partial class ImportForm : Window
    {

        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string directory = "";
        BackgroundWorker worker;

        public ImportForm()
        {
            InitializeComponent();
            btn_stop.IsEnabled = false;
        }

        private void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Title = "Browse Excel Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "Excel",
                Filter = "All Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx",
                FilterIndex = 2,
                RestoreDirectory = true                
                //ReadOnlyChecked = true,
                //ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.txtFile.Text = directory = openFileDialog1.FileName;

            }
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pbStatus.Value = 0;
                if (string.IsNullOrEmpty(this.txtFile.Text))
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "File", "File"), Constant.messageTitileError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!File.Exists(this.txtFile.Text))
                {
                    System.Windows.Forms.MessageBox.Show("File not Exist!", Constant.messageTitileError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                btn_import.IsEnabled = false;
                worker = new BackgroundWorker();
                worker.WorkerSupportsCancellation = true;
                worker.WorkerReportsProgress = true;
                worker.DoWork += Worker_DoWork;
                worker.ProgressChanged += Worker_ProgressChanged;
                worker.RunWorkerAsync();
            }
            catch
            {
                btn_import.IsEnabled = true;
            }
            
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {            
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(directory);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            try
            {

                this.Dispatcher.Invoke(() =>
                {
                    processStatusText.Content = "Loading";
                    btn_stop.IsEnabled = true;
                });
                int rowCount = xlRange.Rows.Count;
                int colCount = xlRange.Columns.Count;
                string serialId = "";
                for (int i = 2; i <= rowCount; i++)
                {
                    if (xlRange.Cells[i, 1] != null && xlRange.Cells[i, 1].Value2 != null)
                    {
                        structExcel structExcel = new structExcel();
                        if (xlRange.Cells[i, 1] != null && xlRange.Cells[i, 1].Value2 != null)
                        {
                            serialId = xlRange.Cells[i, 1].Value2.ToString();
                            structExcel.serialId = serialId;
                        }
                        structExcel.name = xlRange.Cells[i, 2].Value2.ToString();
                        structExcel.name = structExcel.name.ToUpper();
                        structExcel.gender = xlRange.Cells[i, 3].Value2.ToString();
                        string sDate = xlRange.Cells[i, 4].Value2.ToString();
                        double date = double.Parse(sDate);
                        var dateTime = DateTime.FromOADate(date).ToString("MMMM dd, yyyy");
                        structExcel.birthDate = DateTime.Parse(dateTime);
                        structExcel.studentname = xlRange.Cells[i, 5].Value2.ToString();
                        structExcel.email = xlRange.Cells[i, 6].Value2.ToString();
                        structExcel.address = xlRange.Cells[i, 7].Value2.ToString();

                        if (!Constant.listData.ContainsKey(structExcel.serialId))
                        {
                            Constant.listData.Add(structExcel.serialId, structExcel);
                        }
                        else
                        {
                            Console.WriteLine("Duplicated:-"+ structExcel.serialId);
                        }
                    }
                    if (worker.CancellationPending)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            processStatusText.Content = "Stopped";
                            btn_stop.IsEnabled = false;
                        });
                        break;
                    }
                    (sender as BackgroundWorker).ReportProgress((i*100)/rowCount);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Lỗi nhập File hãy kiểm tra lại!", Constant.messageTitileError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                logFile.Error(ex.Message);
            }

            finally
            {
                xlWorkbook.Close();
                xlApp.Quit();
                this.Dispatcher.Invoke(() =>
                {
                    processStatusText.Content = "Finished";
                    btn_import.IsEnabled = true;
                    btn_stop.IsEnabled = false;
                });
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = e.ProgressPercentage;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            worker.CancelAsync();
        }
    }
}
