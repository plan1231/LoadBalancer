using System;
using LoadBalancer.Configuration;
using LoadBalancer.LoadBalancing;
using LoadBalancer.Model;

namespace LoadBalancer.SessionAffinity
{
	public class SessionAffinityMiddleware
	{
        RequestDelegate _next;
        Configuration.SessionAffinity _sessionAffinityConfig;
        private readonly ISessionAffinityPolicy _sessionAffinityPolicy;

        public SessionAffinityMiddleware(RequestDelegate next, Configuration.SessionAffinity sessionAffinityConfig, IEnumerable<ISessionAffinityPolicy> sessionAffinityPolicies)
        {
            _next = next;
            _sessionAffinityConfig = sessionAffinityConfig;
            _sessionAffinityPolicy = sessionAffinityPolicies.Where(x => x.Name == _sessionAffinityConfig.Policy).FirstOrDefault() ??
                throw new InvalidOperationException($"No session affinity policy with name {_sessionAffinityConfig.Policy} found");
        }


        public Task Invoke(HttpContext context)
        {
            ILoadBalancerFeature contextFeature = context.GetLoadBalancerFeature();
            if (_sessionAffinityConfig.Enabled)
            {
                EndpointState? selectedEndpoint = _sessionAffinityPolicy.SelectEndpoint(context, contextFeature.healthyEndpoints);
                if(selectedEndpoint is null)
                {
                    // If no appropriate endpoint, then we let the load balancing middleware select an endpoint further down the line
                    // SetSessionAffinityKey will then be called before the response body is sent back to set the session affinity key of the connection.
                    contextFeature.SetSessionAffinityKey = _sessionAffinityPolicy.AffinitizeResponse;
                }
                else
                {
                    contextFeature.SelectedRemoteEndpoint = selectedEndpoint;
                }
            }
            
            return _next.Invoke(context);

        }
    }
}

