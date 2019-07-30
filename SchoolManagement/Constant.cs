using SchoolManagement.DTO;
using System;
using System.Collections.Generic;
using System.IO;

namespace SchoolManagement
{
    public class Constant
    {
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string userName = "";
        public static int userAuthor = -2;

        public static MainWindow mainWindowPointer;

        public static string messageDuplicated = "{0} is duplicated.";
        public static string messageSaveSucced = "Save operation succeeded.";
        public static string messageSaveFail = "Failed to save. Please try again.";
        public static string messageValidate = "{0} is mandatory. Please enter {1}.";
        public static string messageNothingSelected = "Nothing selected.";
        public static string messageDeleteConfirm = "Do you want to delete the selected {0}?";
        public static string messageDeleteSucced = "Delete operation succeeded.";
        public static string messageDeleteFail = "Failed to delete. Please try again.";
        public static string messageDeleteUse = "Can\'t delete {0} because it has been using on {1}.";
        public static string messageValidateNumber = "{0} must be {1} than {2}.";
        public static string messageNoDataSave = "There is no updated data to save.";

        public static string messageTitileInformation = "Information";
        public static string messageTitileError = "Error";
        public static string messageTitileWarning = "Warning";

        //public static SQLiteConnection m_dbConnection;

        public static void CreateFolderToSaveData()
        {
            try
            {
                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK"))
                {
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK");
                }
                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image"))
                {
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\Image");
                }
                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\DB"))
                {
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\DB");
                    //if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\DB\"+ "Datastore.db"))
                    //{
                    //    File.Copy(Environment.CurrentDirectory + @"\Datastore.db",
                    //        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\DB\Datastore.db",
                    //        true);
                    //}
                }
                if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\DB\" + "Datastore.db"))
                {
                    File.Copy(Environment.CurrentDirectory + @"\Datastore.db",
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ATEK\DB\Datastore.db",
                        false);
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }

        public static Dictionary<string, ProfileRF> listData = new Dictionary<string, ProfileRF>();

        //public enum AccountClass
        //{
        //    Teacher = 0,
        //    Security = 1,
        //    Student = 2,
        //    Guest = 3
        //}

        public enum Gender
        {
            Male = 0,
            Female = 1
        }
    }
}