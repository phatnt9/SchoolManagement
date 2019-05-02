using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.DTO
{
    public class TimeRecord
    {
        private string _ip;
        private string _gate;
        private string _serialId;
        private string _name;
        private string _adno;
        private DateTime _datetime;
        
        public string IP { get => _ip; set => _ip = value; }
        public string NAME { get => _name; set => _name = value; }
        public string ADNO { get => _adno; set => _adno = value; }
        public string PIN_NO { get => _serialId; set => _serialId = value; }
        public string GATE { get => _gate; set => _gate = value; }
        public DateTime TIME_CHECK { get => _datetime; set => _datetime = value; }
    }
}
