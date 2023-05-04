using System;
namespace LoadBalancer.LoadBalancing
{
	public static class LoadBalancingPolicies
	{
        public static string Random => nameof(Random);

        /// <summary>
        /// Select a destination by cycling through them in order.
        /// </summary>
        public static string RoundRobin => nameof(RoundRobin);
    }
}

