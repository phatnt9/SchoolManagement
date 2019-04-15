using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.DTO
{
    public class structExcel
    {
        private string _serialId;
        private string _name;
        private string _gender;
        private DateTime _birthDate;
        private string _studentname;
        private string _email;
        private string _address;
        private string _timeCheck;


        public string serialId { get => _serialId; set => _serialId = value; }
        public string name { get => _name; set => _name = value; }
        public string gender { get => _gender; set => _gender = value; }
        public DateTime birthDate { get => _birthDate; set => _birthDate = value; }
        public string studentname { get => _studentname; set => _studentname = value; }
        public string email { get => _email; set => _email = value; }
        public string address { get => _address; set => _address = value; }
        public string timeCheck { get => _timeCheck; set => _timeCheck = value; }
    }
}
