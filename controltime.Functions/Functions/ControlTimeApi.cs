using controltime.Common.Models;
using controltime.Common.Responses;
using controltime.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace controltime.Functions.Functions
{
    public static class ControlTimeApi
    {
        [FunctionName(nameof(CreateControlTime))]
        public static async Task<IActionResult> CreateControlTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "controltime")] HttpRequest req,
            [Table("controltime", Connection = "AzureWebJobsStorage")] CloudTable controltimeTable,
            ILogger log)
        {
            log.LogInformation("A new time stamp was received");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            ControlTime controltime = JsonConvert.DeserializeObject<ControlTime>(requestBody);

            if (string.IsNullOrEmpty(controltime?.Type) || controltime.Time == default)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = $"The request must have a type (0: Input, 1: Output) and time"
                });
            }

            ControlTimeEntity controltimeEntity = new ControlTimeEntity
            {
                EmployeeID = controltime.EmployeeID,
                InputTime = controltime.Time.ToUniversalTime(),
                ETag = "*",
                Consolidated = false,
                PartitionKey = "CONTROLTIME",
                RowKey = Guid.NewGuid().ToString(),
                Type = controltime.Type
            };

            TableOperation addOperation = TableOperation.Insert(controltimeEntity);
            await controltimeTable.ExecuteAsync(addOperation);

            if (controltime.Type == "0")
            {
                string message = "New employee input time stored in table";
                log.LogInformation(message);

                return new OkObjectResult(new Response
                {
                    IsSuccess = true,
                    Message = message,
                    Result = controltimeEntity
                });
            }
            else
            {
                string message = "New employee output time stored in table";
                log.LogInformation(message);

                return new OkObjectResult(new Response
                {
                    IsSuccess = true,
                    Message = message,
                    Result = controltimeEntity
                });
            }
        }


        [FunctionName(nameof(UpdateControlTime))]
        public static async Task<IActionResult> UpdateControlTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "controltime/{id}")] HttpRequest req,
            [Table("controltime", Connection = "AzureWebJobsStorage")] CloudTable controltimeTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Update for control time: {id}, received");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            ControlTime controltime = JsonConvert.DeserializeObject<ControlTime>(requestBody);

            TableOperation findOperation = TableOperation.Retrieve<ControlTimeEntity>("CONTROLTIME", id);
            TableResult findResult = await controltimeTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Control time not found"
                });
            }

            // Update ControlTime
            ControlTimeEntity controltimeEntity = (ControlTimeEntity)findResult.Result;
            if (!string.IsNullOrEmpty(controltime.Type))
            {
                controltimeEntity.Type = controltime.Type;
            }

            if (controltime.Time != default)
            {
                controltimeEntity.InputTime = controltime.Time;
            }

            TableOperation addOperation = TableOperation.Replace(controltimeEntity);
            await controltimeTable.ExecuteAsync(addOperation);

            string message = $"Control time: {id}, updated in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = controltimeEntity
            });
        }

        [FunctionName(nameof(GetAllControlTimes))]
        public static async Task<IActionResult> GetAllControlTimes(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "controltime")] HttpRequest req,
          [Table("controltime", Connection = "AzureWebJobsStorage")] CloudTable controltimeTable,
          ILogger log)
        {
            log.LogInformation("Get all Control Time received");

            TableQuery<ControlTimeEntity> query = new TableQuery<ControlTimeEntity>();
            TableQuerySegment<ControlTimeEntity> controltimes = await controltimeTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all Control Time";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = controltimes
            });
        }

        [FunctionName(nameof(GetControlTimeById))]
        public static IActionResult GetControlTimeById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "controltime/{id}")] HttpRequest req,
            [Table("controltime", "CONTROLTIME", "{id}", Connection = "AzureWebJobsStorage")] ControlTimeEntity controltimeEntity,
            string id,
            ILogger log)
        {
            log.LogInformation($"Get control time by id: {id}, received");

            if (controltimeEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Control time not found"
                });
            }

            string message = $"Control time: {controltimeEntity.RowKey}, retrieved";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = controltimeEntity
            });
        }

        [FunctionName(nameof(DeleteControlTime))]
        public static async Task<IActionResult> DeleteControlTime(
           [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "controltime/{id}")] HttpRequest req,
           [Table("controltime", "CONTROLTIME", "{id}", Connection = "AzureWebJobsStorage")] ControlTimeEntity controltimeEntity,
           [Table("controltime", Connection = "AzureWebJobsStorage")] CloudTable controltimeTable,
           string id,
           ILogger log)
        {
            log.LogInformation($"Delete Control time: {id}, received");

            if (controltimeEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Control time not found"
                });
            }

            await controltimeTable.ExecuteAsync(TableOperation.Delete(controltimeEntity));
            string message = $"Control time: {controltimeEntity.RowKey}, deleted";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = controltimeEntity
            });
        }

        [FunctionName(nameof(GetConsolidatedMinutes))]
        public static async Task<IActionResult> GetConsolidatedMinutes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "consolidatedtime/{date}")] HttpRequest req,
            [Table("consolidatedtime", Connection = "AzureWebJobsStorage")] CloudTable consolidatedTimeTable,
            DateTime date,
            ILogger log)
        {
            log.LogInformation($"Get consolidated time by date: {date}, received");

            string startDate = TableQuery.GenerateFilterConditionForDate("Date", QueryComparisons.GreaterThanOrEqual, date.ToUniversalTime());
            string endDate = TableQuery.GenerateFilterConditionForDate("Date", QueryComparisons.LessThanOrEqual, date.ToUniversalTime());
            string filter = TableQuery.CombineFilters(startDate, TableOperators.And, endDate);

            TableQuery<ConsolidatedTimeEntity> query = new TableQuery<ConsolidatedTimeEntity>().Where(filter);
            TableQuerySegment<ConsolidatedTimeEntity> completedControlTimes = await consolidatedTimeTable.ExecuteQuerySegmentedAsync(query, null);

            if (!completedControlTimes.Results.Any())
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Consolidated time not found"
                });
            }

            string message = $"Consolidated time: {completedControlTimes.Count()} item(s) retrieved";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = completedControlTimes.Results
            });
        }
    }
}
