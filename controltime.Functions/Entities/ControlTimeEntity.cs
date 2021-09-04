using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace controltime.Functions.Entities
{
    public class ControlTimeEntity : TableEntity
    {
        public int EmployeeID { get; set; }

        public DateTime InputTime { get; set; }

        public DateTime OutputTime { get; set; }

        public string Type { get; set; }

        public bool Consolidated { get; set; }
    }
}
