using System;
namespace LoadBalancer.SessionAffinity
{
	public static class SessionAffinityPolicies
	{

        /// <summary>
        /// Use an unencrypted cookie to store the remote endpoint
        /// </summary>
        public static string SimpleCookie => nameof(SimpleCookie);
    }
}

