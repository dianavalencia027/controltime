using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace controltime.Functions.Entities
{
    public class ConsolidatedTimeEntity : TableEntity
    {
        public int EmployeeID { get; set; }

        public DateTime Date { get; set; }

        public int MinutesWorked { get; set; }
    }
}
