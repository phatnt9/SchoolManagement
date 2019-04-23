using SchoolManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SchoolManagement.Constant;

namespace SchoolManagement.DTO
{
    public class DeviceRF
    {
        private int _id;
        private string _ip;
        private string _accountClass;

        public int ID { get => _id; set => _id = value; }
        public string IP { get => _ip; set => _ip = value; }
        public string CLASS { get => _accountClass; set => _accountClass = value; }

        public DeviceItem deviceItem;
    }
}
