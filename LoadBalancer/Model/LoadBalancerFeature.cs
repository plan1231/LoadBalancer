using System;

namespace LoadBalancer.Model
{
	public class LoadBalancerFeature : ILoadBalancerFeature
	{

        public EndpointState? SelectedRemoteEndpoint { get; set; }

        public IReadOnlyList<EndpointState> healthyEndpoints { get; set; } = default!;
        public Action<EndpointState>? SetSessionAffinityKey { get; set; } = default;
        Action<HttpContext, EndpointState>? ILoadBalancerFeature.SetSessionAffinityKey { get; set; } = default;
    }
}

