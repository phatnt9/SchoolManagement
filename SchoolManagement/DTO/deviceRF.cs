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
        private string _ip;
        private AccountClass _accountClass;

        public string ip { get => _ip; set => _ip = value; }
        public AccountClass accountClass { get => _accountClass; set => _accountClass = value; }
    }
}
