using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LoadBalancer.Configuration;
using LoadBalancer.Model;
using Microsoft.Extensions.Hosting;

namespace LoadBalancer.HealthCheck
{
    public class ActiveHealthCheckMonitor : BackgroundService
    {
        private readonly Configuration.HealthCheck _healthCheckConfig;
        private readonly HttpClient _httpClient;
        private readonly StateManager _stateManager;

        public ActiveHealthCheckMonitor(Configuration.HealthCheck healthCheck, HttpClient httpClient, StateManager stateManager)
        {
            _healthCheckConfig = healthCheck;
            _httpClient = httpClient;
            _stateManager = stateManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(_healthCheckConfig.Interval), stoppingToken);

                await CheckAllEndpointsHealth();
            }
        }

        public async Task CheckAllEndpointsHealth()
        {
            foreach (var endpoint in _stateManager.EndpointStates)
            {
                await CheckEndpointHealth(endpoint);
            }
        }

        private async Task CheckEndpointHealth(EndpointState endpoint)
        {
            try
            {
                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(_healthCheckConfig.Timeout));
                var response = await _httpClient.GetAsync(endpoint.Address, cancellationTokenSource.Token);
                endpoint.IsHealthy = response.IsSuccessStatusCode;
            }
            catch
            {
                endpoint.IsHealthy = false;
            }
        }
    }

}
