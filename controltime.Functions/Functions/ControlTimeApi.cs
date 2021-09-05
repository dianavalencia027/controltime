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

            if (string.IsNullOrEmpty(controltime?.Type))
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have a type (0: Input, 1: Output)"
                });
            }

            ControlTimeEntity controltimeEntity = new ControlTimeEntity
            {
                EmployeeID = controltime.EmployeeID,
                InputTime = DateTime.UtcNow,
                OutputTime = DateTime.UtcNow,
                ETag = "*",
                Consolidated = false,
                PartitionKey = "CONTROLTIME",
                RowKey = Guid.NewGuid().ToString(),
                Type = controltime.Type
            };

            TableOperation addOperation = TableOperation.Insert(controltimeEntity);
            await controltimeTable.ExecuteAsync(addOperation);


            //TODO: Check this conditional

            //string message = "New time stored in table";
            //log.LogInformation(message);

            if (controltime.Type == "0")
            {
                return new OkObjectResult(new Response
                {
                    IsSuccess = true,
                    //Message = message,
                    message = "New input time stored in table",
                    Result = controltimeEntity
                });
            }
            else
            {
                return new OkObjectResult(new Response
                {
                    IsSuccess = true,
                    //Message = message,
                    message = "New out time stored in table",
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

            // Validate ControlTime x id
            TableOperation findOperation = TableOperation.Retrieve<ControlTimeEntity>("TODO", id);
            TableResult findResult = await controltimeTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Control time not found"
                });
            }

            // Update todo
            ControlTimeEntity controltimeEntity = (ControlTimeEntity)findResult.Result;
            controltimeEntity.Consolidated = controltimeEntity.Consolidated;
            if (!string.IsNullOrEmpty(controltime.Type))
            {
                controltimeEntity.Type = controltime.Type;
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



    }
}
