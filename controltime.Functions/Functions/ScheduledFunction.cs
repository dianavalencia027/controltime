using controltime.Functions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace controltime.Functions.Functions
{
    public static class ScheduledFunction
    {
        [FunctionName("ScheduledFunction")]
        public static async Task Run(
            [TimerTrigger("* * * * *")] TimerInfo myTimer,
            [Table("controltime", Connection = "AzureWebJobsStorage")] CloudTable controltimeTable,
            [Table("consolidatedtime", Connection = "AzureWebJobsStorage")] CloudTable consolidatedTimeTable,
            ILogger log)
        {
            log.LogInformation($"Calculating Minutes Worked");

            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            TableQuery<ControlTimeEntity> query = new TableQuery<ControlTimeEntity>().Where(filter);
            TableQuerySegment<ControlTimeEntity> completedControlTimes = await controltimeTable.ExecuteQuerySegmentedAsync(query, null);

            foreach (var completedcontrolTime in completedControlTimes.GroupBy(c => c.EmployeeID))
            {
                ConsolidatedTimeEntity consolidatedTimeEntity = new ConsolidatedTimeEntity
                {
                    EmployeeID = completedcontrolTime.Key,
                    Date = DateTime.Parse(completedcontrolTime.First().InputTime.ToShortDateString()),
                    MinutesWorked = await GetMinutesWorked(completedcontrolTime.ToList(), controltimeTable),
                    ETag = "*",
                    PartitionKey = "CONSOLIDATEDTIME",
                    RowKey = Guid.NewGuid().ToString(),
                };

                if (consolidatedTimeEntity.MinutesWorked > 0)
                {
                    await consolidatedTimeTable.ExecuteAsync(TableOperation.Insert(consolidatedTimeEntity));
                }
            }

            log.LogInformation($"To be continued...");

        }

        private static async Task<int> GetMinutesWorked(List<ControlTimeEntity> completedcontrolTime, CloudTable controltimeTable)
        {
            completedcontrolTime = completedcontrolTime.OrderBy(c => c.Timestamp).ToList();
            List<ControlTimeEntity> input = completedcontrolTime.Where(c => c.Type == "0").ToList();
            List<ControlTimeEntity> output = completedcontrolTime.Where(c => c.Type == "1").ToList();

            if (input.Count != output.Count)
            {
                input.RemoveAt(input.Count - 1);
            }

            TimeSpan minutesWorked = default;

            for (int i = 0; i < input.Count; i++)
            {
                minutesWorked += output[i].InputTime.Subtract(input[i].InputTime);
            }

            if (minutesWorked != default)
            {
                var controlTimeToUpdateList = input.Concat(output);

                foreach (var controlTimeToUpdate in controlTimeToUpdateList)
                {
                    controlTimeToUpdate.Consolidated = true;
                    TableOperation updateOperation = TableOperation.Replace(controlTimeToUpdate);
                    await controltimeTable.ExecuteAsync(updateOperation);
                }
            }

            return int.Parse(minutesWorked.TotalMinutes.ToString());
        }
    }
}
