using System;
namespace LoadBalancer.Model
{
	public interface ILoadBalancerFeature
	{
		public EndpointState? SelectedRemoteEndpoint { get; set; }
		public IReadOnlyList<EndpointState> healthyEndpoints { get; set; }
        public Action<HttpContext, EndpointState>? SetSessionAffinityKey { get; set; }
    }
}

