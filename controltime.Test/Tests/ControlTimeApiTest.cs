using controltime.Common.Models;
using controltime.Functions.Entities;
using controltime.Functions.Functions;
using controltime.Test.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace controltime.Test.Tests
{
    public class ControlTimeApiTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void CreateControlTime_Should_Return_200()
        {
            // Arrange
            MockCloudTable mockTable = new MockCloudTable(new Uri("http://127.0.0.1:10002/devstoreaccount1/controltime"));

            ControlTime controlTimeRequest = TestFactory.GetControlTimeRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(controlTimeRequest);

            // Act
            IActionResult response = await ControlTimeApi.CreateControlTime(request, mockTable, logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }


        [Fact]
        public async void UpdateControlTime_Should_Return_200()
        {
            // Arrange
            MockCloudTable mockTable = new MockCloudTable(new Uri("http://127.0.0.1:10002/devstoreaccount1/controltime"));
            Guid controlTimeId = Guid.NewGuid();
            ControlTime controlTimeRequest = TestFactory.GetControlTimeRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(controlTimeId, controlTimeRequest);

            // Act
            IActionResult response = await ControlTimeApi.UpdateControlTime(request, mockTable, controlTimeId.ToString(), logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public void GetControlTimeById_Should_Return_200()
        {
            // Arrange
            Guid controlTimeId = Guid.NewGuid();
            ControlTimeEntity controlTimeEntity = TestFactory.GetControlTimeEntity();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(controlTimeId, controlTimeEntity);

            // Act
            IActionResult response = ControlTimeApi.GetControlTimeById(request, controlTimeEntity, controlTimeId.ToString(), logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void DeleteControlTime_Should_Return_200()
        {
            // Arrange
            MockCloudTable mockTable = new MockCloudTable(new Uri("http://127.0.0.1:10002/devstoreaccount1/controltime"));
            Guid controlTimeId = Guid.NewGuid();
            ControlTimeEntity controlTimeEntity = TestFactory.GetControlTimeEntity();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(controlTimeId, controlTimeEntity);

            // Act
            IActionResult response = await ControlTimeApi.DeleteControlTime(request, controlTimeEntity, mockTable, controlTimeId.ToString(), logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
    }
}
