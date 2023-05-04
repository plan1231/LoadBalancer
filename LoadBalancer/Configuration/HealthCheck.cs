using System;
namespace LoadBalancer.Configuration
{
	public sealed record HealthCheck
	{
        public bool Enabled { get; init; } = false;
        // How often should we actively probe the remote endpoints, in seconds
        public int Interval { get; init; } = 30;
        // After how long of a hanging connection should we time out and deem the endpoint as unhealthy, in seconds
        public int Timeout { get; init; } = 30;

    }
}

