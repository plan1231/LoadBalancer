using System;
using LoadBalancer.Model;
namespace LoadBalancer.LoadBalancing
{
	public interface ILoadBalancingPolicy
	{
		public string Name { get; }
		public EndpointState? SelectEndpoint(IReadOnlyList<EndpointState> availableEndpoints);
    }
}

