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
    public class MainWindowModel
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow mainW;
        public ListCollectionView groupedAccount { get; private set; }
        public ListCollectionView groupedTimeCheck { get; private set; }
        public List<structExcel> accountsList;
        public List<Person> timeCheckList;
        public List<DateTime> timeCheckDataGrid;

        public MainWindowModel(MainWindow mainW)
        {
            this.mainW = mainW;
            accountsList = new List<structExcel>();
            timeCheckList = new List<Person>();
            timeCheckDataGrid = new List<DateTime>();
            groupedAccount = (ListCollectionView)CollectionViewSource.GetDefaultView(accountsList);
            groupedTimeCheck = (ListCollectionView)CollectionViewSource.GetDefaultView(timeCheckDataGrid);
        }

        public void LoadTimeCheckFromExcel()
        {
            try
            {
                string fileNameTimeCheck = "TimeCheck.xlsx";
                string pathTimeCheck = Path.Combine(Environment.CurrentDirectory, fileNameTimeCheck);
                mainW.pbStatus.Value = 0;
                if (string.IsNullOrEmpty(pathTimeCheck))
                {
                    System.Windows.Forms.MessageBox.Show(String.Format(Constant.messageValidate, "File", "File"), Constant.messageTitileError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!File.Exists(pathTimeCheck))
                {
                    System.Windows.Forms.MessageBox.Show("File not Exist!", Constant.messageTitileError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                mainW.workerTimeCheck = new BackgroundWorker();
                mainW.workerTimeCheck.WorkerSupportsCancellation = true;
                mainW.workerTimeCheck.WorkerReportsProgress = true;
                mainW.workerTimeCheck.DoWork += WorkerTimeCheck_DoWork; ;
                mainW.workerTimeCheck.ProgressChanged += WorkerTimeCheck_ProgressChanged; ;
                mainW.workerTimeCheck.RunWorkerCompleted += WorkerTimeCheck_RunWorkerCompleted; ; ;
                mainW.workerTimeCheck.RunWorkerAsync(argument: pathTimeCheck);
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }
        public void LoadProfileFromExcel()
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
                mainW.workerProfile = new BackgroundWorker();
                mainW.workerProfile.WorkerSupportsCancellation = true;
                mainW.workerProfile.WorkerReportsProgress = true;
                mainW.workerProfile.DoWork += WorkerProfile_DoWork;
                mainW.workerProfile.ProgressChanged += WorkerProfile_ProgressChanged;
                mainW.workerProfile.RunWorkerCompleted += WorkerProfile_RunWorkerCompleted; ;
                mainW.workerProfile.RunWorkerAsync(argument: path);
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }
        private void WorkerProfile_DoWork(object sender, DoWorkEventArgs e)
        {
            string pathProfile = (string)e.Argument;

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(pathProfile);
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
                        structExcel.Class = xlRange.Cells[i, 3].Value2.ToString();
                        structExcel.gender = xlRange.Cells[i, 4].Value2.ToString();
                        string sDate = xlRange.Cells[i, 5].Value2.ToString();
                        double date = double.Parse(sDate);
                        var dateTime = DateTime.FromOADate(date).ToString("MMMM dd, yyyy");
                        structExcel.birthDate = DateTime.Parse(dateTime);
                        structExcel.studentname = xlRange.Cells[i, 6].Value2.ToString();
                        structExcel.email = xlRange.Cells[i, 7].Value2.ToString();
                        structExcel.address = xlRange.Cells[i, 8].Value2.ToString();

                        if (!Constant.listData.ContainsKey(structExcel.serialId))
                        {
                            Constant.listData.Add(structExcel.serialId, structExcel);
                        }
                        else
                        {
                            Console.WriteLine("Duplicated:-" + structExcel.serialId);
                        }
                    }
                    if (mainW.workerProfile.CancellationPending)
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
        private void WorkerTimeCheck_DoWork(object sender, DoWorkEventArgs e)
        {
            string pathTimeCheck = (string)e.Argument;

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(pathTimeCheck);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;
            try
            {

                int rowCount = xlRange.Rows.Count;
                int colCount = xlRange.Columns.Count;
                string serialId = "";
                for (int i = 1; i <= rowCount; i++)
                {
                    if (xlRange.Cells[i, 2] != null && xlRange.Cells[i, 2].Value2 != null)
                    {
                        Person structExcel = new Person();
                        if (xlRange.Cells[i, 2] != null && xlRange.Cells[i, 2].Value2 != null)
                        {
                            serialId = xlRange.Cells[i, 2].Value2.ToString();
                            structExcel.serialId = serialId;
                        }
                        structExcel.tick = xlRange.Cells[i, 4].Value2.ToString();

                        //string sDate = xlRange.Cells[i, 4].Value2.ToString();
                        //double date = double.Parse(sDate);
                        //var dateTime = DateTime.FromOADate(date).ToString("MMMM dd, yyyy");
                        timeCheckList.Add(structExcel);


                    }
                    if (mainW.workerTimeCheck.CancellationPending)
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
        private void WorkerProfile_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            mainW.pbStatus.Value = e.ProgressPercentage;
        }
        private void WorkerTimeCheck_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            mainW.pbStatus.Value = e.ProgressPercentage;
        }
        private void WorkerProfile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
            LoadTimeCheckFromExcel();
        }
        private void WorkerTimeCheck_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
            mainW.pbStatus.Value = 0;
            AddTimeCheckToData();
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
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }

        }

        public void CreateListTimeCheck()
        {
            if (groupedTimeCheck.IsEditingItem)
                groupedTimeCheck.CommitEdit();
            if (groupedTimeCheck.IsAddingNew)
                groupedTimeCheck.CommitNew();
            groupedTimeCheck.Refresh();
        }

        public List<string> GetListSerialId()
        {
            List<string> listSerial = new List<string>();
            if (Constant.listData.Count != 0)
            {
                try
                {
                    foreach (KeyValuePair<string, structExcel> entry in Constant.listData)
                    {
                        // do something with entry.Value or entry.Key
                        listSerial.Add(entry.Key);
                    }
                }
                catch (Exception ex)
                {
                    logFile.Error(ex.Message);
                }
            }
            return listSerial;

        }
        public void AddTimeCheckToData()
        {
            try
            {
                foreach (Person person in timeCheckList)
                {
                    if (Constant.listData.ContainsKey(person.serialId))
                    {
                        string[] timeArray = person.tick.Split(';');
                        foreach(string tick in timeArray)
                        {
                            long date = long.Parse(tick);
                            //DateTime dateTime = DateTime.FromOADate(date);
                            DateTime dateTime = new DateTime(date);
                            Constant.listData[person.serialId].timeCheck.Add(dateTime);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }
        }

        public void LoadTimeCheck(string serialId)
        {
            timeCheckDataGrid.Clear();
            mainW.workerDatagridTimeCheck = new BackgroundWorker();
            mainW.workerDatagridTimeCheck.WorkerSupportsCancellation = true;
            mainW.workerDatagridTimeCheck.WorkerReportsProgress = true;
            mainW.workerDatagridTimeCheck.DoWork += WorkerDatagridTimeCheck_DoWork; ;
            mainW.workerDatagridTimeCheck.ProgressChanged += WorkerDatagridTimeCheck_ProgressChanged; ;
            mainW.workerDatagridTimeCheck.RunWorkerCompleted += WorkerDatagridTimeCheck_RunWorkerCompleted; ; ;
            mainW.workerDatagridTimeCheck.RunWorkerAsync(argument: serialId);
        }

        private void WorkerDatagridTimeCheck_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
            mainW.pbStatus.Value = 0;
            CreateListTimeCheck();
        }

        private void WorkerDatagridTimeCheck_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            mainW.pbStatus.Value = e.ProgressPercentage;
        }

        private void WorkerDatagridTimeCheck_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string selectedSerialId = (string)e.Argument;
                if (Constant.listData.ContainsKey(selectedSerialId))
                {
                    int maxI = Constant.listData[selectedSerialId].timeCheck.Count;
                    for (int i = 0; i < maxI; i++)
                    {
                        timeCheckDataGrid.Add(Constant.listData[selectedSerialId].timeCheck[i]);
                        (sender as BackgroundWorker).ReportProgress((i * 100) / maxI);
                    }
                    
                }
                
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
            }

            finally
            {

            }
        }

        bool CheckinServer(List<Person> person)
        {
            try
            {
                foreach (Person p in person)
                {
                    if (Constant.listData.ContainsKey(p.serialId))
                    {
                        string[] timeArray = p.tick.Split(';');
                        foreach (string tick in timeArray)
                        {
                            long date = long.Parse(tick);
                            //DateTime dateTime = DateTime.FromOADate(date);
                            DateTime dateTime = new DateTime(date);
                            Constant.listData[p.serialId].timeCheck.Add(dateTime);
                        }
                        //============

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                return false;
            }

            finally
            {
                
            }

            
        }
    }
}
