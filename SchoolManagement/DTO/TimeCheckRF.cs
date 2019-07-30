using System;

namespace SchoolManagement.DTO
{
    public class TimeCheckRF
    {
        private string _id;
        private string _serialId;
        private DateTime _datetime;

        public string ID { get => _id; set => _id = value; }
        public string PIN_NO { get => _serialId; set => _serialId = value; }
        public DateTime TIME_CHECK { get => _datetime; set => _datetime = value; }
    }
}