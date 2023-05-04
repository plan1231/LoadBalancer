using System;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LoadBalancer.HealthCheck;
using LoadBalancer.Model;
using LoadBalancer.Configuration;
using Moq;
using Moq.Protected;
using Xunit;
namespace LoadBalancer.Tests.HealthCheck
{

    public class ActiveHealthCheckMonitorTests
    {
        [Fact]
        public async Task CheckAllEndpointsHealth_AllHealthyEndpoints_UpdatesStatusToHealthy()
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK);
            var stateManager = new StateManager(new LoadBalancer.Configuration.LoadBalancer
            {
                Endpoints = { "https://healthy-endpoint-1.test", "https://healthy-endpoint-2.test" }
            });

            var healthCheckConfig = new Configuration.HealthCheck
            {
                Timeout = 1
            };

            var healthCheckMonitor = new ActiveHealthCheckMonitor(healthCheckConfig, httpClient, stateManager);

            await healthCheckMonitor.CheckAllEndpointsHealth();

            Assert.All(stateManager.EndpointStates, endpoint => Assert.True(endpoint.IsHealthy));
        }

        [Fact]
        public async Task CheckAllEndpointsHealth_AllUnhealthyEndpoints_UpdatesStatusToUnhealthy()
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.InternalServerError);
            var stateManager = new StateManager(new LoadBalancer.Configuration.LoadBalancer
            {
                Endpoints = { "https://unhealthy-endpoint-1.test", "https://unhealthy-endpoint-2.test" }
            });

            var healthCheckConfig = new Configuration.HealthCheck
            {
                Timeout = 1
            };

            var healthCheckMonitor = new ActiveHealthCheckMonitor(healthCheckConfig, httpClient, stateManager);

            await healthCheckMonitor.CheckAllEndpointsHealth();

            Assert.All(stateManager.EndpointStates, endpoint => Assert.False(endpoint.IsHealthy));
        }

        [Fact]
        public async Task CheckAllEndpointsHealth_MixedEndpoints_UpdatesStatusCorrectly()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
                {
                    return request.RequestUri.Host.Contains("unhealthy")
                    ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    : new HttpResponseMessage(HttpStatusCode.OK);
                });
            var httpClient = new HttpClient(handlerMock.Object);

            var stateManager = new StateManager(new LoadBalancer.Configuration.LoadBalancer
            {
                Endpoints = { "https://healthy-endpoint-1.test", "https://unhealthy-endpoint-1.test" }
            });

            var healthCheckConfig = new Configuration.HealthCheck
            {
                Timeout = 1
            };

            var healthCheckMonitor = new ActiveHealthCheckMonitor(healthCheckConfig, httpClient, stateManager);

            await healthCheckMonitor.CheckAllEndpointsHealth();

            Assert.True(stateManager.EndpointStates.First(endpoint => endpoint.Address.Contains("healthy")).IsHealthy);
            Assert.False(stateManager.EndpointStates.First(endpoint => endpoint.Address.Contains("unhealthy")).IsHealthy);
        }

        private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(statusCode));

            return new HttpClient(handlerMock.Object);
        }
    }

}


