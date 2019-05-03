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
        private static readonly log4net.ILog logFile = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

        public static List<ProfileRF> LoadProfileRF(string name = "", string pinno = "", string adno = "")
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var p = new DynamicParameters();
                p.Add("@NAME", "%" + name + "%");
                p.Add("@PIN_NO", "%" + pinno + "%");
                p.Add("@ADNO", "%" + adno + "%");

                var output = cnn.Query<ProfileRF>("SELECT * FROM RF_PROFILE WHERE ((NAME LIKE (@NAME)) AND (PIN_NO LIKE (@PIN_NO)) AND (ADNO LIKE (@ADNO)))", p);
                return output.ToList();
            }
        }

        public static List<TimeRecord> LoadTimeCheckRF(string PIN_NO, DateTime time, string ip = null)
        {
            try
            {
                if (ip == null)
                {
                    if (time != DateTime.MinValue)
                    {
                        using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                        {
                            var p = new DynamicParameters();
                            p.Add("@PIN_NO", PIN_NO);
                            p.Add("@FROM", time);
                            p.Add("@TO", time.AddDays(1));

                            var output = cnn.Query<TimeRecord>("SELECT * FROM RF_TIMECHECK INNER JOIN RF_PROFILE,RF_DEVICE ON " +
                                "(RF_PROFILE.PIN_NO = RF_TIMECHECK.PIN_NO AND RF_DEVICE.IP = RF_TIMECHECK.IP)" +
                                " WHERE (TIME_CHECK >= @FROM AND TIME_CHECK < @TO) AND (RF_PROFILE.PIN_NO = @PIN_NO)", p);
                            return output.ToList();
                        }
                    }
                    else
                    {
                        using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                        {
                            var p = new DynamicParameters();
                            p.Add("@PIN_NO", PIN_NO);

                            var output = cnn.Query<TimeRecord>("SELECT * FROM RF_TIMECHECK INNER JOIN RF_PROFILE,RF_DEVICE ON " +
                                "(RF_PROFILE.PIN_NO = RF_TIMECHECK.PIN_NO AND RF_DEVICE.IP = RF_TIMECHECK.IP)" +
                                " WHERE (RF_PROFILE.PIN_NO = @PIN_NO)", p);
                            return output.ToList();
                        }
                    }
                }
                else
                {
                    if (time != DateTime.MinValue)
                    {
                        using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                        {
                            var p = new DynamicParameters();
                            p.Add("@PIN_NO", PIN_NO);
                            p.Add("@IP", ip);
                            p.Add("@FROM", time);
                            p.Add("@TO", time.AddDays(1));

                            var output = cnn.Query<TimeRecord>("SELECT * FROM RF_TIMECHECK INNER JOIN RF_PROFILE,RF_DEVICE ON " +
                                "(RF_PROFILE.PIN_NO = RF_TIMECHECK.PIN_NO AND RF_DEVICE.IP = RF_TIMECHECK.IP)" +
                                " WHERE (TIME_CHECK >= @FROM AND TIME_CHECK < @TO) AND (RF_DEVICE.IP = @IP)", p);
                            return output.ToList();
                        }
                    }
                    else
                    {
                        using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                        {
                            var p = new DynamicParameters();
                            p.Add("@PIN_NO", PIN_NO);
                            p.Add("@IP", ip);
                            //RF_PROFILE.PIN_NO,TIME_CHECK,RF_DEVICE.IP,GATE
                            var output = cnn.Query<TimeRecord>("SELECT * FROM RF_TIMECHECK INNER JOIN RF_PROFILE,RF_DEVICE ON " +
                                "(RF_PROFILE.PIN_NO = RF_TIMECHECK.PIN_NO AND RF_DEVICE.IP = RF_TIMECHECK.IP)" +
                                " WHERE (RF_DEVICE.IP = @IP)", p);
                            return output.ToList();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
                return new List<TimeRecord>();
            }

        }




        public static void SaveDeviceRF(DeviceRF deviceRF)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("INSERT INTO RF_DEVICE (IP,GATE,CLASS,STATUS) VALUES (@IP, @GATE, @CLASS,@STATUS)", deviceRF);
            }
        }

        public static void SaveProfileRF(ProfileRF accountRFCard)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("INSERT INTO RF_PROFILE (PIN_NO,NAME,CLASS,GENDER,DOB,EMAIL,ADDRESS,PHONE,ADNO,DISU,STATUS,LOCK_DATE,IMAGE) " +
                    "VALUES (@PIN_NO,@NAME,@CLASS,@GENDER,@DOB,@EMAIL,@ADDRESS,@PHONE,@ADNO,@DISU,@STATUS,@LOCK_DATE,@IMAGE)", accountRFCard);
            }
        }

        public static void SaveTimeCheckRF(string IP, string PIN_NO, DateTime TIME_CHECK)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var p = new DynamicParameters();
                p.Add("@PIN_NO", PIN_NO);
                p.Add("@TIME_CHECK", TIME_CHECK);
                p.Add("@IP", IP);
                cnn.Execute("INSERT INTO RF_TIMECHECK (PIN_NO,TIME_CHECK,IP) VALUES (@PIN_NO, @TIME_CHECK, @IP)", p);
            }
        }

        public static void UpdateDeviceRF(string ip, string status = "", string CLASS = "", string GATE = "")
        {
            try
            {
                if (status != "")
                {
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        var p = new DynamicParameters();
                        p.Add("@STATUS", status);
                        p.Add("@IP", ip);
                        cnn.Execute("UPDATE RF_DEVICE SET " +
                                "STATUS = @STATUS " +
                                "WHERE IP = @IP", p);
                    }
                }
                else
                {
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        var p = new DynamicParameters();
                        p.Add("@CLASS", CLASS);
                        p.Add("@GATE", GATE);
                        p.Add("@IP", ip);
                        cnn.Execute("UPDATE RF_DEVICE SET " +
                                "CLASS = @CLASS, " +
                                "GATE = @GATE " +
                                "WHERE IP = @IP", p);
                    }
                }
                
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
            }
        }


        public static void UpdateProfileRF(ProfileRF profileRF,string Status = null)
        {
            try
            {
                if (Status == null)
                {
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        cnn.Execute("UPDATE RF_PROFILE SET " +
                            "name = @NAME, " +
                            "CLASS = @CLASS, " +
                            "GENDER = @GENDER, " +
                            "DOB = @DOB, " +
                            "EMAIL = @EMAIL, " +
                            "ADDRESS = @ADDRESS, " +
                            "PHONE = @PHONE, " +
                            "ADNO = @ADNO, " +
                            "DISU = @DISU, " +
                            "IMAGE = @IMAGE " +
                            "WHERE PIN_NO = @PIN_NO", profileRF);
                    }
                }
                else
                {
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        cnn.Execute("UPDATE RF_PROFILE SET " +
                            "name = @NAME, " +
                            "CLASS = @CLASS, " +
                            "GENDER = @GENDER, " +
                            "DOB = @DOB, " +
                            "EMAIL = @EMAIL, " +
                            "ADDRESS = @ADDRESS, " +
                            "PHONE = @PHONE, " +
                            "ADNO = @ADNO, " +
                            "DISU = @DISU, " +
                            "IMAGE = @IMAGE, " +
                            "LOCK_DATE = @LOCK_DATE, " +
                            "STATUS = @STATUS " +
                            "WHERE PIN_NO = @PIN_NO", profileRF);
                    }
                }
            }
            catch (Exception ex)
            {
                logFile.Error(ex.Message);
                Constant.mainWindowPointer.WriteLog(ex.Message);
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
                        var outputFilter = cnn.Query<string>("SELECT PIN_NO FROM RF_PROFILE WHERE (RF_PROFILE.CLASS = @CLASS AND RF_PROFILE.STATUS = 'Active')", filter);
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
