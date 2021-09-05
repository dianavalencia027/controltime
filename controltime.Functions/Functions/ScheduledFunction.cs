using controltime.Functions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace controltime.Functions.Functions
{
    public static class ScheduledFunction
    {
        //TODO: Validate time calculation
        [FunctionName("ScheduledFunction")]
        public static async Task Run(
            [TimerTrigger("0 */2 * * * *")] TimerInfo myTimer,
            [Table("controltime", Connection = "AzureWebJobsStorage")] CloudTable controltimeTable,
            ILogger log)
        {
            log.LogInformation($"Deleting completed function executed at: {DateTime.Now}");

            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, true);
            TableQuery<ControlTimeEntity> query = new TableQuery<ControlTimeEntity>().Where(filter);
            TableQuerySegment<ControlTimeEntity> completedControlTimes = await controltimeTable.ExecuteQuerySegmentedAsync(query, null);
            int deleted = 0;
            foreach (ControlTimeEntity completedcontrolTime in completedControlTimes)
            {
                await controltimeTable.ExecuteAsync(TableOperation.Delete(completedcontrolTime));
                deleted++;
            }

            log.LogInformation($"Deleted: {deleted} items at: {DateTime.Now}");

        }
    }
}
