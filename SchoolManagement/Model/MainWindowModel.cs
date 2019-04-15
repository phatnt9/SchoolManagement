using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Forms;
using System.Threading;
using SchoolManagement.DTO;
using Excel = Microsoft.Office.Interop.Excel;

namespace SchoolManagement.Model
{
    class MainWindowModel
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow mainW;
        public ListCollectionView groupedAccount { get; private set; }
        public List<structExcel> accountsList;

        public MainWindowModel(MainWindow mainW)
        {
            this.mainW = mainW;
            accountsList = new List<structExcel>();
            groupedAccount = (ListCollectionView)CollectionViewSource.GetDefaultView(accountsList);
        }
        

        public void LoadDataFromExcel()
        {
            try
            {
                string fileName = "DataImport.xlsx";
                string path = Path.Combine(Environment.CurrentDirectory, fileName);
                mainW.pbStatus.Value = 0;
                if (string.IsNullOrEmpty(path))
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "File", "File"), Constant.messageTitileError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!File.Exists(path))
                {
                    System.Windows.Forms.MessageBox.Show("File not Exist!", Constant.messageTitileError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                mainW.worker = new BackgroundWorker();
                mainW.worker.WorkerSupportsCancellation = true;
                mainW.worker.WorkerReportsProgress = true;
                mainW.worker.DoWork += Worker_DoWork;
                mainW.worker.ProgressChanged += Worker_ProgressChanged;
                mainW.worker.RunWorkerCompleted += Worker_RunWorkerCompleted; ;
                mainW.worker.RunWorkerAsync();
            }
            catch
            {
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Check to see if an error occurred in the
            // background process.
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
                return;
            }

            // Check to see if the background process was cancelled.
            if (e.Cancelled)
            {
                MessageBox.Show("Processing cancelled.");
                return;
            }

            // Everything completed normally.
            // process the response using e.Result
            //MessageBox.Show("Processing is complete.");
            CreateListAccount();
            mainW.pbStatus.Value = 0;

        }

        public void CreateListAccount()
        {
            try
            {
                foreach (KeyValuePair<string, structExcel> entry in Constant.listData)
                {
                    // do something with entry.Value or entry.Key
                    accountsList.Add(entry.Value);
                }
                if (groupedAccount.IsEditingItem)
                    groupedAccount.CommitEdit();
                if (groupedAccount.IsAddingNew)
                    groupedAccount.CommitNew();
                groupedAccount.Refresh();

                if (mainW.AccountListData.HasItems)
                {
                    mainW.AccountListData.SelectedItem = mainW.AccountListData.Items[0];
                }
            }
            catch
            {

            }
            
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string fileName = "DataImport.xlsx";
            string path = Path.Combine(Environment.CurrentDirectory, fileName);
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(path);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;
            try
            {
                
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
                            Console.WriteLine("Duplicated:-" + structExcel.serialId);
                        }
                    }
                    if (mainW.worker.CancellationPending)
                    {
                        break;
                    }
                    (sender as BackgroundWorker).ReportProgress((i * 100) / rowCount);
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
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            mainW.pbStatus.Value = e.ProgressPercentage;
        }





    }
}
