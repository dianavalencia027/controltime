using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using controltime.Common.Models;
using controltime.Common.Responses;
using controltime.Functions.Entities;

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
                EmployeeID= controltime.EmployeeID,
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


    }
}
