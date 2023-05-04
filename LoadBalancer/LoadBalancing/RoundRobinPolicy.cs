using System;
using System.Diagnostics.Metrics;
using LoadBalancer.Model;
using LoadBalancer.Utilities;
namespace LoadBalancer.LoadBalancing
{
	public class RoundRobinPolicy : ILoadBalancingPolicy
	{
        private readonly AtomicCounter _counter = new AtomicCounter();

        public string Name => LoadBalancingPolicies.RoundRobin;

        public EndpointState? SelectEndpoint(IReadOnlyList<EndpointState> availableEndpoints)
        {
            if (availableEndpoints.Count == 0)
            {
                return null;
            }

            // Increment returns the new value and we want the first return value to be 0.
            var offset = _counter.Increment() - 1;

            // Use nonnegative indices only as negative ones will result in an exception.
            return availableEndpoints[(offset & 0x7FFFFFFF) % availableEndpoints.Count];
        }
    }
}