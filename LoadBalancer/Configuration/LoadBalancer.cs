using LoadBalancer.LoadBalancing;
namespace LoadBalancer.Configuration
{
    public sealed record LoadBalancer
    {
        public HealthCheck HealthCheck { get; init; } = new HealthCheck();
        public SessionAffinity SessionAffinity { get; init; } = new SessionAffinity();
        public string Policy { get; init; } = LoadBalancingPolicies.RoundRobin;
        public List<string> Endpoints { get; init; } = new List<string>();
	}
}

