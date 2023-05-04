using System;
using LoadBalancer.Configuration;
using LoadBalancer.Model;

namespace LoadBalancer.SessionAffinity
{
	public interface ISessionAffinityPolicy
    {
        public string Name { get; }
        public EndpointState? SelectEndpoint(HttpContext context, IReadOnlyList<EndpointState> availableEndpoints);
        public void AffinitizeResponse(HttpContext context, EndpointState selectedEndpoint);
    }
}

