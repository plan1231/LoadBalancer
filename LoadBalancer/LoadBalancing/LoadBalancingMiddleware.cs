using System;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using LoadBalancer.Model;
using LoadBalancer.Configuration;
using LoadBalancer.Utilities;
namespace LoadBalancer.LoadBalancing
{
    
    public class LoadBalancingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly StateManager _stateManager;
        private readonly ILoadBalancingPolicy _loadBalancingPolicy;
        private readonly Configuration.LoadBalancer _loadBalancerConfig;

        public LoadBalancingMiddleware(RequestDelegate next, StateManager stateManager, Configuration.LoadBalancer loadBalancerConfig,IEnumerable<ILoadBalancingPolicy> loadBalancingPolicies)
        {
            _next = next;
            _stateManager = stateManager;
            _loadBalancerConfig = loadBalancerConfig;
            _loadBalancingPolicy = loadBalancingPolicies.FirstOrDefault(x => x.Name == loadBalancerConfig.Policy) ??
                throw new InvalidOperationException($"No load balancing policy with name {loadBalancerConfig.Policy} was found");

        }

        public Task InvokeAsync(HttpContext context)
        {
            ILoadBalancerFeature contextFeature = context.GetLoadBalancerFeature();

            // Only use load balancing policy to select a remote endpoint if the session affinity middleware didn't already do so
            if(contextFeature.SelectedRemoteEndpoint is null)
            {
                IReadOnlyList<EndpointState> healthyEndpoints = contextFeature.healthyEndpoints;

                // Select an endpoint based on load balancing algorithm 
                contextFeature.SelectedRemoteEndpoint = _loadBalancingPolicy.SelectEndpoint(healthyEndpoints);
            }
         
            return _next.Invoke(context);
        }
    }

}

