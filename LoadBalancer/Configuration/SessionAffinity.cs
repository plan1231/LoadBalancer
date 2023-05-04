using System;
using LoadBalancer.SessionAffinity;
namespace LoadBalancer.Configuration
{
	public sealed record SessionAffinity
	{
		public bool Enabled { get; init; } = false;
		public string Policy { get; init; } = SessionAffinityPolicies.SimpleCookie;
		public string AffinityKeyName { get; init; } = "AffinityKey";
	}
}

