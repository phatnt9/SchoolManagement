using System;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.Generic;
using SchoolManagement.DTO;
using System.ComponentModel;
using System.Threading;
using Newtonsoft.Json;

namespace SchoolManagement.Form
{
    /// <summary>
    /// Interaction logic for ImportForm.xaml
    /// </summary>
    public partial class ImportForm : Window
    {

        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string importFilePath = "";
        private string importFileFolder = "";
        BackgroundWorker worker;
        MainWindow mainW;

        public ImportForm(MainWindow mainW)
        {
            InitializeComponent();
            this.mainW = mainW;
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
                this.txtFile.Text = importFilePath = openFileDialog1.FileName;
                importFileFolder = System.IO.Path.GetDirectoryName(openFileDialog1.FileName);
                string fileName = "DataImport.xlsx";
                string path = Path.Combine(Environment.CurrentDirectory, fileName);
                Console.WriteLine(path);
                //FileInfo fileInfo = new FileInfo("DataImport");
                //Console.WriteLine(fileInfo.DirectoryName);
            }
        }

        public void CreateFolderToSaveImage(string importFolderPath)
        {
            if (!Directory.Exists(importFolderPath + @"\Image"))
            {
                Directory.CreateDirectory(importFolderPath + @"\Image");
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
                worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                worker.ProgressChanged += Worker_ProgressChanged;
                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                btn_import.IsEnabled = true;
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }

        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // check error, check cancel, then use result
            if (e.Error != null)
            {
                // handle the error
                processStatusText.Content = "Error";
            }
            else if (e.Cancelled)
            {
                // handle cancellation
                processStatusText.Content = "";
            }
            else
            {
                

            }
            // general cleanup code, runs when there was an error or not.
            mainW.mainModel.ReloadListProfileRFDGV();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(importFilePath);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;
            CreateFolderToSaveImage(importFileFolder);
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
                    if (xlRange.Cells[i, 2] != null && xlRange.Cells[i, 2].Value2 != null)
                    {
                        ProfileRF profile = new ProfileRF();
                        if (xlRange.Cells[i, 8] != null && xlRange.Cells[i, 8].Value2 != null)
                        {
                            serialId = xlRange.Cells[i, 8].Value2.ToString();
                            profile.PIN_NO = serialId;
                        }
                        profile.NAME = xlRange.Cells[i, 2].Value2.ToString().ToUpper();
                        profile.ADNO = xlRange.Cells[i, 3].Value2.ToString().ToUpper();
                        profile.GENDER = (xlRange.Cells[i, 4].Value2.ToString()=="Male"? Constant.Gender.Male : Constant.Gender.Female);

                        string sDate = xlRange.Cells[i, 5].Value2.ToString();
                        double date = double.Parse(sDate);
                        var dateTime = DateTime.FromOADate(date).ToString("MMMM dd, yyyy");
                        profile.DOB = DateTime.Parse(dateTime);

                        sDate = xlRange.Cells[i, 6].Value2.ToString();
                        date = double.Parse(sDate);
                        dateTime = DateTime.FromOADate(date).ToString("MMMM dd, yyyy");
                        profile.DISU = DateTime.Parse(dateTime);


                        profile.IMAGE = xlRange.Cells[i, 7].Value2.ToString();


                        profile.CLASS = xlRange.Cells[i, 9].Value2.ToString();
                        profile.EMAIL = (xlRange.Cells[i, 10].Value2 == null) ? "" : xlRange.Cells[i, 10].Value2.ToString();
                        profile.ADDRESS = (xlRange.Cells[i, 11].Value2 == null) ? "" : xlRange.Cells[i, 11].Value2.ToString();
                        profile.PHONE = (xlRange.Cells[i, 12].Value2 == null) ? "" : xlRange.Cells[i, 12].Value2.ToString();
                        profile.STATUS = xlRange.Cells[i, 13].Value2.ToString();
                        if (profile.STATUS == "Suspended")
                        {
                            sDate = xlRange.Cells[i, 14].Value2.ToString();
                            date = double.Parse(sDate);
                            dateTime = DateTime.FromOADate(date).ToString("MMMM dd, yyyy");
                            profile.LOCK_DATE = DateTime.Parse(dateTime);
                        }

                        try
                        {
                            SqliteDataAccess.SaveProfileRF(profile);
                        }
                        catch (Exception ex)
                        {
                            //System.Windows.Forms.MessageBox.Show("Lỗi nhập File hãy kiểm tra lại!", Constant.messageTitileError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            logFile.Error(ex.Message);
                            Constant.mainWindowPointer.WriteLog(ex.Message);
                        }
                        //if (!Constant.listData.ContainsKey(structExcel.PIN_NO))
                        //{
                        //    Constant.listData.Add(structExcel.PIN_NO, structExcel);
                        //}
                        //else
                        //{
                        //    Console.WriteLine("Duplicated:-" + structExcel.PIN_NO);
                        //}
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
                    (sender as BackgroundWorker).ReportProgress((i * 100) / rowCount);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Lỗi nhập File hãy kiểm tra lại!", Constant.messageTitileError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }

            finally
            {
                Constant.mainWindowPointer.WriteLog("Dong xlWorkbook.Close();");
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
