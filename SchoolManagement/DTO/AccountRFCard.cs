using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.DTO
{
    public class ProfileRF
    {
        private int _id;
        private string _serialId;
        private string _name;
        private Constant.AccountClass _class;
        private Constant.Gender _gender;
        private DateTime _birthDate;
        private string _studentname;
        private string _email;
        private string _address;
        private string _phone;


        public int ID { get => _id; set => _id = value; }
        public string SERIAL_ID { get => _serialId; set => _serialId = value; }
        public string NAME { get => _name; set => _name = value; }
        public Constant.AccountClass CLASS { get => _class; set => _class = value; }
        public Constant.Gender GENDER { get => _gender; set => _gender = value; }
        public DateTime BIRTHDAY { get => _birthDate; set => _birthDate = value; }
        public string STUDENT { get => _studentname; set => _studentname = value; }
        public string EMAIL { get => _email; set => _email = value; }
        public string ADDRESS { get => _address; set => _address = value; }
        public string PHONE { get => _phone; set => _phone = value; }
        

    }
}
