using System.Collections.Generic;
using LoadBalancer.LoadBalancing;
using LoadBalancer.Model;
using Xunit;

namespace LoadBalancer.Tests.LoadBalancing
{
    public class RoundRobinPolicyTests
    {
        [Fact]
        public void SelectEndpoint_RoundRobin_ReturnsNextEndpoint()
        {
            var availableEndpoints = new List<EndpointState>
            {
                new EndpointState("https://endpoint-1.test"),
                new EndpointState("https://endpoint-2.test"),
                new EndpointState("https://endpoint-3.test")
            };

            var policy = new RoundRobinPolicy();

            Assert.Equal(availableEndpoints[0], policy.SelectEndpoint(availableEndpoints));
            Assert.Equal(availableEndpoints[1], policy.SelectEndpoint(availableEndpoints));
            Assert.Equal(availableEndpoints[2], policy.SelectEndpoint(availableEndpoints));
            Assert.Equal(availableEndpoints[0], policy.SelectEndpoint(availableEndpoints));
            Assert.Equal(availableEndpoints[1], policy.SelectEndpoint(availableEndpoints));
        }

        [Fact]
        public void SelectEndpoint_NoAvailableEndpoints_ReturnsNull()
        {
            var availableEndpoints = new List<EndpointState>();
            var policy = new RoundRobinPolicy();

            var selectedEndpoint = policy.SelectEndpoint(availableEndpoints);

            Assert.Null(selectedEndpoint);
        }
    }
}
