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

        public static List<DateTime> LoadTimeCheckRF(string serialID, DateTime time)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var p = new DynamicParameters();
                p.Add("@SERIAL_ID", serialID);
                p.Add("@FROM", time);
                p.Add("@TO", time.AddDays(1));


                var output = cnn.Query<DateTime>("SELECT TIMECHECK FROM RF_TIMECHECK INNER JOIN RF_PROFILE ON " +
                    "RF_PROFILE.SERIAL_ID = RF_TIMECHECK.SERIAL_ID WHERE (TIMECHECK >= @FROM AND TIMECHECK < @TO) AND (RF_PROFILE.SERIAL_ID = @SERIAL_ID)", p);
                return output.ToList();
            }
        }

        public static void SaveDeviceRF(DeviceRF deviceRF)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("INSERT INTO RF_DEVICE (IP,CLASS) VALUES (@IP, @CLASS)", deviceRF);
            }
        }

        public static void SaveProfileRF(ProfileRF accountRFCard)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("INSERT INTO RF_PROFILE (SERIAL_ID,NAME,CLASS,GENDER,BIRTHDAY,STUDENT,EMAIL,ADDRESS,PHONE) " +
                    "VALUES (@SERIAL_ID,@NAME,@CLASS,@GENDER,@BIRTHDAY,@STUDENT,@EMAIL,@ADDRESS,@PHONE)", accountRFCard);
            }
        }

        public static void SaveTimeCheckRF(string SERIAL_ID, DateTime TIMECHECK)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var p = new DynamicParameters();
                p.Add("@SERIAL_ID", SERIAL_ID);
                p.Add("@TIMECHECK", TIMECHECK);
                cnn.Execute("INSERT INTO RF_TIMECHECK (SERIAL_ID,TIMECHECK) VALUES (@SERIAL_ID, @TIMECHECK)", p);
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
                cnn.Execute("DELETE FROM RF_PROFILE WHERE SERIAL_ID=@SERIAL_ID", profileRF);
            }
        }


        public static List<string> LoadListProfileRFSerialId(string ip)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var p = new DynamicParameters();
                p.Add("@IP", ip);

                var output = cnn.Query<string>("SELECT SERIAL_ID FROM RF_PROFILE INNER JOIN RF_DEVICE ON " +
                    "RF_PROFILE.CLASS = RF_DEVICE.CLASS WHERE (RF_DEVICE.IP = @IP)", p);
                return output.ToList();
            }
        }

    }
}
