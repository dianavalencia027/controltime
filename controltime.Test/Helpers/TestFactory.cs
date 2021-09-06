using controltime.Common.Models;
using controltime.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.IO;

namespace controltime.Test.Helpers
{
    public class TestFactory
    {
        public static ControlTimeEntity GetControlTimeEntity()
        {
            return new ControlTimeEntity
            {
                ETag = "*",
                PartitionKey = "CONTROLTIME",
                RowKey = Guid.NewGuid().ToString(),
                InputTime = DateTime.UtcNow,
                Consolidated = false,
                Type = "0",
                EmployeeID = 1
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid controlTimeId, ControlTime controlRequest)
        {
            string request = JsonConvert.SerializeObject(controlRequest);
            DefaultHttpRequest httpRequest = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{controlTimeId}"
            };

            return httpRequest;
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid controlTimeId, ControlTimeEntity controlTimeEntity)
        {
            string request = JsonConvert.SerializeObject(controlTimeEntity);
            DefaultHttpRequest httpRequest = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{controlTimeId}"
            };

            return httpRequest;
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid controlTimeId)
        {
            DefaultHttpRequest httpRequest = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{controlTimeId}"
            };

            return httpRequest;
        }

        public static DefaultHttpRequest CreateHttpRequest(ControlTime controlTimeRequest)
        {
            string request = JsonConvert.SerializeObject(controlTimeRequest);
            DefaultHttpRequest httpRequest = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request)
            };

            return httpRequest;
        }

        public static DefaultHttpRequest CreateHttpRequest()
        {
            DefaultHttpRequest httpRequest = new DefaultHttpRequest(new DefaultHttpContext())
            {
            };

            return httpRequest;
        }

        public static ControlTime GetControlTimeRequest()
        {
            return new ControlTime
            {
                Time = DateTime.UtcNow,
                Consolidated = false,
                Type = "0",
                EmployeeID = 1
            };
        }

        public static Stream GenerateStreamFromString(string stringToConvert)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(stringToConvert);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;

            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }

            return logger;
        }
    }
}
