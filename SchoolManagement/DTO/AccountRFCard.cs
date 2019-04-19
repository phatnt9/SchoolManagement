using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.DTO
{
    public class AccountRFCard
    {
        private string _serialId;
        private string _name;
        private string _class;
        private string _gender;
        private DateTime _birthDate;
        private string _studentname;
        private string _email;
        private string _address;
        private List<DateTime> _timeCheck;


        public string serialId { get => _serialId; set => _serialId = value; }
        public string name { get => _name; set => _name = value; }
        public string Class { get => _class; set => _class = value; }
        public string gender { get => _gender; set => _gender = value; }
        public DateTime birthDate { get => _birthDate; set => _birthDate = value; }
        public string studentname { get => _studentname; set => _studentname = value; }
        public string email { get => _email; set => _email = value; }
        public string address { get => _address; set => _address = value; }
        public List<DateTime> timeCheck { get => _timeCheck; set => _timeCheck = value; }

        public AccountRFCard()
        {
            timeCheck = new List<DateTime>();
        }

    }
}
