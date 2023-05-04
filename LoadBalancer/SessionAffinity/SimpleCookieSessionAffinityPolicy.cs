using System;
using LoadBalancer.Model;

namespace LoadBalancer.SessionAffinity
{
    public class SimpleCookieSessionAffinityPolicy : ISessionAffinityPolicy
    {
        public string Name => SessionAffinityPolicies.SimpleCookie;

        private readonly Configuration.SessionAffinity _sessionAffinityConfig;

        public SimpleCookieSessionAffinityPolicy(Configuration.SessionAffinity sessionAffinityConfig)
        {
            _sessionAffinityConfig = sessionAffinityConfig;
        }

        public void AffinitizeResponse(HttpContext context, EndpointState selectedEndpoint)
        {
            var affinityKey = selectedEndpoint.Address;
            context.Response.Cookies.Append(_sessionAffinityConfig.AffinityKeyName, affinityKey);
        }


        public EndpointState? SelectEndpoint(HttpContext context, IReadOnlyList<EndpointState> availableEndpoints)
        {
            if (!context.Request.Cookies.TryGetValue(_sessionAffinityConfig.AffinityKeyName, out var affinityKey))
            {
                return null;
            }

            foreach (var endpoint in availableEndpoints)
            {
                if (endpoint.Address == affinityKey)
                {
                    return endpoint;
                }
            }
            return null;
        }
    }
}

