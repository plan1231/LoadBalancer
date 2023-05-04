using System;
using System.Threading;

namespace LoadBalancer.Model
{
    public sealed class EndpointState
    {
        public string Address { get; init; }
        private int _isHealthy;
        public bool IsHealthy
        {
            get => Interlocked.CompareExchange(ref _isHealthy, 0, 0) == 1;
            set => Interlocked.Exchange(ref _isHealthy, value ? 1 : 0);
        }

        public EndpointState(string address)
        {
            Address = address;
            IsHealthy = true;
        }
    }
}
