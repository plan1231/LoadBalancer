using System;
using LoadBalancer.Model;
using LoadBalancer.Configuration;
namespace LoadBalancer.HealthCheck
{
	public class HealthCheckMiddleware
	{
        StateManager _stateManager;
        RequestDelegate _next;
        Configuration.HealthCheck _healthCheckConfig;

		public HealthCheckMiddleware(RequestDelegate next, StateManager stateManager, Configuration.HealthCheck healthCheckConfig)
		{
            _next = next;
            _stateManager = stateManager;
            _healthCheckConfig = healthCheckConfig;
		}

		public Task Invoke(HttpContext context)
		{
            if (_healthCheckConfig.Enabled)
            {
                var healthyEndpoints = _stateManager.EndpointStates.Where(e => e.IsHealthy).ToList();
                context.GetLoadBalancerFeature().healthyEndpoints = healthyEndpoints;
            }
            else
            {
                // If health check not enabled, then we assume all endpoints are healthy.
                context.GetLoadBalancerFeature().healthyEndpoints = _stateManager.EndpointStates;
            }

            // We do not care if the list of healthy endpoints is empty. We will deal with it in the innermost middleware: the ForwardingMiddleware
            return _next.Invoke(context);
        }
	}
}

