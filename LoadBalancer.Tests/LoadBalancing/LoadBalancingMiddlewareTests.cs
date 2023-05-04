using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoadBalancer.LoadBalancing;
using LoadBalancer.Model;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace LoadBalancer.Tests.LoadBalancing
{
    public class LoadBalancingMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_SessionAffinityNotSet_SelectsEndpointUsingPolicy()
        {
            var stateManager = new StateManager(new LoadBalancer.Configuration.LoadBalancer
            {
                Endpoints = { "https://endpoint-1.test", "https://endpoint-2.test" }
            });

            var loadBalancerConfig = new LoadBalancer.Configuration.LoadBalancer
            {
                Policy = LoadBalancingPolicies.RoundRobin
            };

            var policy = new RoundRobinPolicy();
            var middleware = new LoadBalancingMiddleware(_ => Task.CompletedTask, stateManager, loadBalancerConfig, new[] { policy });

            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set<ILoadBalancerFeature>(new LoadBalancerFeature
            {
                healthyEndpoints = stateManager.EndpointStates
            });

            await middleware.InvokeAsync(httpContext);

            var selectedRemoteEndpoint = httpContext.GetLoadBalancerFeature().SelectedRemoteEndpoint;
            Assert.NotNull(selectedRemoteEndpoint);
        }

        [Fact]
        public async Task InvokeAsync_SessionAffinitySet_DoesNotOverrideEndpoint()
        {
            var stateManager = new StateManager(new LoadBalancer.Configuration.LoadBalancer
            {
                Endpoints = { "https://endpoint-1.test", "https://endpoint-2.test" }
            });

            var loadBalancerConfig = new LoadBalancer.Configuration.LoadBalancer
            {
                Policy = LoadBalancingPolicies.RoundRobin
            };

            var policy = new RoundRobinPolicy();
            var middleware = new LoadBalancingMiddleware(_ => Task.CompletedTask, stateManager, loadBalancerConfig, new[] { policy });

            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set<ILoadBalancerFeature>(new LoadBalancerFeature
            {
                healthyEndpoints = stateManager.EndpointStates,
                SelectedRemoteEndpoint = stateManager.EndpointStates.Last()
            });

            await middleware.InvokeAsync(httpContext);

            var selectedRemoteEndpoint = httpContext.GetLoadBalancerFeature().SelectedRemoteEndpoint;
            Assert.Equal(stateManager.EndpointStates.Last(), selectedRemoteEndpoint);
        }
    }
}
