using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.DTO
{
    public class SqliteDataAccess
    {
        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }

        public static List<DeviceRF> LoadDeviceRF()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<DeviceRF>("SELECT * FROM RF_DEVICE", new DynamicParameters());
                return output.ToList();
            }
        }

        public static List<ProfileRF> LoadProfileRF()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<ProfileRF>("SELECT * FROM RF_PROFILE", new DynamicParameters());
                return output.ToList();
            }
        }

        public static List<DateTime> LoadTimeCheckRF(string PIN_NO, DateTime time)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var p = new DynamicParameters();
                p.Add("@PIN_NO", PIN_NO);
                p.Add("@FROM", time);
                p.Add("@TO", time.AddDays(1));
                
                var output = cnn.Query<DateTime>("SELECT TIME_CHECK FROM RF_TIMECHECK INNER JOIN RF_PROFILE ON " +
                    "RF_PROFILE.PIN_NO = RF_TIMECHECK.PIN_NO WHERE (TIME_CHECK >= @FROM AND TIME_CHECK < @TO) AND (RF_PROFILE.PIN_NO = @PIN_NO)", p);
                return output.ToList();
            }
        }




        public static void SaveDeviceRF(DeviceRF deviceRF)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("INSERT INTO RF_DEVICE (IP,GATE,CLASS) VALUES (@IP, @GATE, @CLASS)", deviceRF);
            }
        }

        public static void SaveProfileRF(ProfileRF accountRFCard)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("INSERT INTO RF_PROFILE (PIN_NO,NAME,CLASS,GENDER,DOB,STUDENT,EMAIL,ADDRESS,PHONE,ADNO,DISU,STATUS) " +
                    "VALUES (@PIN_NO,@NAME,@CLASS,@GENDER,@DOB,@STUDENT,@EMAIL,@ADDRESS,@PHONE,@ADNO,@DISU,@STATUS)", accountRFCard);
            }
        }

        public static void SaveTimeCheckRF(string PIN_NO, DateTime TIME_CHECK)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var p = new DynamicParameters();
                p.Add("@SERIAL_ID", PIN_NO);
                p.Add("@TIME_CHECK", TIME_CHECK);
                cnn.Execute("INSERT INTO RF_TIMECHECK (PIN_NO,TIME_CHECK) VALUES (@PIN_NO, @TIME_CHECK)", p);
            }
        }


        public static void UpdateProfileRF(ProfileRF profileRF)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("UPDATE RF_PROFILE SET " +
                    "name = @NAME, " +
                    "CLASS = @CLASS, " +
                    "GENDER = @GENDER, " +
                    "DOB = @DOB, " +
                    "STUDENT = @STUDENT, " +
                    "EMAIL = @EMAIL, " +
                    "ADDRESS = @ADDRESS, " +
                    "PHONE = @PHONE, " +
                    "ADNO = @ADNO, " +
                    "DISU = @DISU, " +
                    "STATUS = @STATUS " +
                    "WHERE PIN_NO = @PIN_NO", profileRF);
            }
        }

        
        public static void RemoveDeviceRF(DeviceRF deviceRF)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("DELETE FROM RF_DEVICE WHERE IP=@IP", deviceRF);
            }
        }

        public static void RemoveProfileRF(ProfileRF profileRF)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("DELETE FROM RF_TIMECHECK WHERE PIN_NO=@PIN_NO", profileRF);
                cnn.Execute("DELETE FROM RF_PROFILE WHERE PIN_NO=@PIN_NO", profileRF);
            }
        }




        public static List<string> LoadListProfileRFSerialId(string ip)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var p = new DynamicParameters();
                p.Add("@IP", ip);

                var output = cnn.Query<string>("SELECT CLASS FROM RF_DEVICE WHERE (RF_DEVICE.IP = @IP)", p);
                string[] classArray = output.ToList()[0].Split(',');
                List<string> returnSerialIdList = new List<string>();
                foreach (string Class in classArray)
                {
                    if (Class != "")
                    {
                        var filter = new DynamicParameters();
                        filter.Add("@CLASS", Class);
                        var outputFilter = cnn.Query<string>("SELECT PIN_NO FROM RF_PROFILE WHERE (RF_PROFILE.CLASS = @CLASS)", filter);
                        outputFilter.ToList();
                        foreach (string item in outputFilter)
                        {
                            returnSerialIdList.Add(item);
                        }
                    }
                }
                return returnSerialIdList;
            }
        }

    }
}
