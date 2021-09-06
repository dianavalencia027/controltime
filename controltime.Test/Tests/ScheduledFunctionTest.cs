using controltime.Functions.Functions;
using controltime.Test.Helpers;
using System;
using Xunit;

namespace controltime.Test.Tests
{
    public class ScheduledFunctionTest
    {
        [Fact]
        public async void ScheduledFunction_Should_Log_Message()
        {
            // Arrange
            MockCloudTable mockControlTime = new MockCloudTable(new Uri("http://127.0.0.1:10002/devstoreaccount1/controltime"));
            MockCloudTable mockConsolidatedTime = new MockCloudTable(new Uri("http://127.0.0.1:10002/devstoreaccount1/consolidatedtime"));
            ListLogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);

            // Act
            await ScheduledFunction.Run(null, mockControlTime, mockConsolidatedTime, logger);
            string message = logger.Logs[0];

            // Assert
            Assert.Contains("Calculating Minutes Worked", message);
        }
    }
}
