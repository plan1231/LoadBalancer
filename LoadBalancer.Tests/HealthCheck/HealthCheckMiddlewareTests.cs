using System.Linq;
using System.Threading.Tasks;
using LoadBalancer.HealthCheck;
using LoadBalancer.Model;
using LoadBalancer.Configuration;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace LoadBalancer.Tests.HealthCheck
{
    public class HealthCheckMiddlewareTests
    {
        [Fact]
        public async Task Invoke_HealthCheckEnabled_SetsHealthyEndpoints()
        {
            var stateManager = new StateManager(new LoadBalancer.Configuration.LoadBalancer
            {
                Endpoints = { "https://endpoint-1.test", "https://endpoint-2.test" }
            });

            stateManager.EndpointStates.First().IsHealthy = true;
            stateManager.EndpointStates.Last().IsHealthy = false;

            var healthCheckConfig = new Configuration.HealthCheck { Enabled = true };

            var middleware = new HealthCheckMiddleware(_ => Task.CompletedTask, stateManager, healthCheckConfig);

            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set<ILoadBalancerFeature>(new LoadBalancerFeature());

            await middleware.Invoke(httpContext);

            var healthyEndpoints = httpContext.GetLoadBalancerFeature().healthyEndpoints;
            Assert.Single(healthyEndpoints);
            Assert.True(healthyEndpoints.First().IsHealthy);
        }

        [Fact]
        public async Task Invoke_HealthCheckDisabled_SetsAllEndpointsAsHealthy()
        {
            var stateManager = new StateManager(new LoadBalancer.Configuration.LoadBalancer
            {
                Endpoints = { "https://endpoint-1.test", "https://endpoint-2.test" }
            });

            stateManager.EndpointStates.First().IsHealthy = true;
            stateManager.EndpointStates.Last().IsHealthy = false;

            var healthCheckConfig = new Configuration.HealthCheck { Enabled = false };


            var middleware = new HealthCheckMiddleware(_ => Task.CompletedTask, stateManager, healthCheckConfig);

            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set<ILoadBalancerFeature>(new LoadBalancerFeature());

            await middleware.Invoke(httpContext);

            var healthyEndpoints = httpContext.GetLoadBalancerFeature().healthyEndpoints;
            Assert.Equal(2, healthyEndpoints.Count);
        }
    }
}
