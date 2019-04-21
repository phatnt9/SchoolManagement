using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.DTO
{
    public class TimeCheckRF
    {
        private string _id;
        private string _serialId;
        private DateTime _datetime;

        public string ID { get => _id; set => _id = value; }
        public string SERIAL_ID { get => _serialId; set => _serialId = value; }
        public DateTime TIMECHECK { get => _datetime; set => _datetime = value; }
    }
}
