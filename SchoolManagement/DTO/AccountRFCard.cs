﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.DTO
{
    public class ProfileRF
    {
        private int _id;
        private string _pinNo;
        private string _name;
        private string _class;
        private Constant.Gender _gender;
        private DateTime _dob;
        private string _email;
        private string _address;
        private string _phone;
        private string _adno;
        private DateTime _disu;
        private DateTime _lockDate;
        private string _status;


        public int ID { get => _id; set => _id = value; }
        public string PIN_NO { get => _pinNo; set => _pinNo = value; }
        public string NAME { get => _name; set => _name = value; }
        public string CLASS { get => _class; set => _class = value; }
        public Constant.Gender GENDER { get => _gender; set => _gender = value; }
        public DateTime DOB { get => _dob; set => _dob = value; }
        public string EMAIL { get => _email; set => _email = value; }
        public string ADDRESS { get => _address; set => _address = value; }
        public string PHONE { get => _phone; set => _phone = value; }
        public string ADNO { get => _adno; set => _adno = value; }
        public DateTime DISU { get => _disu; set => _disu = value; }
        public DateTime LOCK_DATE { get => _lockDate; set => _lockDate = value; }
        public string STATUS { get => _status; set => _status = value; }
        

    }
}
