using LoadBalancer.Configuration;
using LoadBalancer.LoadBalancing;
using LoadBalancer.SessionAffinity;
using LoadBalancer.HealthCheck;
using LoadBalancer.Forwarding;
using LoadBalancer.Model;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Set up configuration
var configuration = builder.Configuration;
var loadBalancerConfig = new LoadBalancer.Configuration.LoadBalancer();
configuration.GetSection("LoadBalancer").Bind(loadBalancerConfig);
builder.Services.AddSingleton(loadBalancerConfig);
// Register child-level configuration elements so that objects have direct access
// Makes writing unit tests less wordy as you don't need to init unrelated config objects
builder.Services.AddSingleton(loadBalancerConfig.HealthCheck);
builder.Services.AddSingleton(loadBalancerConfig.SessionAffinity);


// Add misc singletons
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<StateManager>();

// Add active health checking if enabled
// Add the ActiveHealthCheckMonitor if enabled
if (loadBalancerConfig.HealthCheck.Enabled)
{
    builder.Services.AddHostedService<ActiveHealthCheckMonitor>();
}

// Add load balancing policies
builder.Services.TryAddEnumerable(new[] {
            ServiceDescriptor.Singleton<ILoadBalancingPolicy, RoundRobinPolicy>(),
        });

// Add session affinity policies
builder.Services.TryAddEnumerable(new[]
{
    ServiceDescriptor.Singleton<ISessionAffinityPolicy, SimpleCookieSessionAffinityPolicy>(),
});


var app = builder.Build();

app.UseMiddleware<PipelineInitializerMiddleware>();
app.UseMiddleware<HealthCheckMiddleware>();
app.UseMiddleware<SessionAffinityMiddleware>();
app.UseMiddleware<LoadBalancingMiddleware>();
app.UseMiddleware<ForwardingMiddleware>();

app.Run();

