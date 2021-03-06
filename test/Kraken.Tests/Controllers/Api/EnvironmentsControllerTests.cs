﻿using Kraken.Controllers.Api;
using Kraken.Services;
using Moq;
using Octopus.Client.Model;
using Xunit;

namespace Kraken.Tests.Controllers.Api
{
    public class EnvironmentsControllerTests
    {
        [Fact]
        public void TestGetEnvironmentsReturnsOctopusData()
        {
            var octopusApi = new Mock<IOctopusProxy>(MockBehavior.Strict);
            octopusApi.Setup(o => o.GetEnvironments()).Returns(new[] { new EnvironmentResource() });

            var controller = new EnvironmentsController(octopusApi.Object);

            var result = controller.GetEnvironments();

            Assert.NotNull(result);

            octopusApi.VerifyAll();
        }
    }
}
