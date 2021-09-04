using System;

namespace controltime.Common.Models
{
    public class ControlTime
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string Type { get; set; }

        public bool Consolidated { get; set; }
    }
}
