using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace controltime.Functions.Entities
{
    public class ControlTimeEntity : TableEntity
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string Type { get; set; }

        public bool Consolidated { get; set; }
    }
}
