using System;

namespace controltime.Common.Models
{
    public class ControlTime
    {
        public int EmployeeID { get; set; }

        public DateTime Time { get; set; }

        //public DateTime OutputTime { get; set; }

        public string Type { get; set; }

        public bool Consolidated { get; set; }
    }
}
