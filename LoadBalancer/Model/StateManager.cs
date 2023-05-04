using System;
using LoadBalancer.Configuration;
namespace LoadBalancer.Model
{
	public class StateManager
	{
		public IReadOnlyList<EndpointState> EndpointStates { get => endpointStates_; }
		private List<EndpointState> endpointStates_ = new List<EndpointState>();

		public StateManager(Configuration.LoadBalancer config)
		{
			if (config.Endpoints.Count < 1) throw new InvalidOperationException("The configuration must provide at least 1 remote endpoint");
			foreach(string endpoint in config.Endpoints)
			{
				endpointStates_.Add(new EndpointState(endpoint));
			}
		}
	}
}

