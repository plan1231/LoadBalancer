using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoadBalancer.Configuration;
using LoadBalancer.Model;
using LoadBalancer.SessionAffinity;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using static System.Net.WebRequestMethods;

namespace LoadBalancer.Tests.SessionAffinity
{
    public class SessionAffinityMiddlewareTests
    {
        [Fact]
        public async Task Invoke_SessionAffinityEnabledButNoEndpointFound_SetsSetSessionAffinityKey()
        {
            // Arrange
            var sessionAffinityConfig = new Configuration.SessionAffinity
            {
                Enabled = true,
                AffinityKeyName = "AffinityKey",
                Policy = SessionAffinityPolicies.SimpleCookie
            };

            var policy = new SimpleCookieSessionAffinityPolicy(sessionAffinityConfig);
            var sessionAffinityPolicies = new List<ISessionAffinityPolicy> { policy };
            var next = new Mock<RequestDelegate>();
            var middleware = new SessionAffinityMiddleware(next.Object, sessionAffinityConfig, sessionAffinityPolicies);

            var httpContext = new DefaultHttpContext();
            httpContext.SetLoadBalancerFeature(new LoadBalancerFeature());
            httpContext.Request.Headers["Cookie"] = "UnrelatedCookie=value";

            // Act
            await middleware.Invoke(httpContext);

            // Assert
            var contextFeature = httpContext.GetLoadBalancerFeature();
            Assert.NotNull(contextFeature.SetSessionAffinityKey);
        }

        [Fact]
        public async Task Invoke_SessionAffinityEnabledAndEndpointFound_SetsSelectedRemoteEndpoint()
        {
            // Arrange
            var sessionAffinityConfig = new Configuration.SessionAffinity
            {
                Enabled = true,
                AffinityKeyName = "AffinityKey",
                Policy = SessionAffinityPolicies.SimpleCookie
            };

            var policy = new SimpleCookieSessionAffinityPolicy(sessionAffinityConfig);
            var sessionAffinityPolicies = new List<ISessionAffinityPolicy> { policy };
            var next = new Mock<RequestDelegate>();
            var middleware = new SessionAffinityMiddleware(next.Object, sessionAffinityConfig, sessionAffinityPolicies);

            var httpContext = new DefaultHttpContext();
            httpContext.SetLoadBalancerFeature(new LoadBalancerFeature());
            httpContext.Request.Headers["Cookie"] = "AffinityKey=https%3A%2F%2Ftest1-endpoint";

            var availableEndpoints = new List<EndpointState>
            {
                new EndpointState("https://test1-endpoint"),
                new EndpointState("https://test2-endpoint")
            };
            httpContext.GetLoadBalancerFeature().healthyEndpoints = availableEndpoints;

            // Act
            await middleware.Invoke(httpContext);

            // Assert
            var contextFeature = httpContext.GetLoadBalancerFeature();
            Assert.Equal(availableEndpoints[0], contextFeature.SelectedRemoteEndpoint);
        }

        [Fact]
        public async Task Invoke_SessionAffinityDisabled_DoesNotModifyLoadBalancerFeature()
        {
            // Arrange
            var sessionAffinityConfig = new Configuration.SessionAffinity
            {
                Enabled = false,
                AffinityKeyName = "AffinityKey",
                Policy = SessionAffinityPolicies.SimpleCookie
            };

            var policy = new SimpleCookieSessionAffinityPolicy(sessionAffinityConfig);
            var sessionAffinityPolicies = new List<ISessionAffinityPolicy> { policy };
            var next = new Mock<RequestDelegate>();
            var middleware = new SessionAffinityMiddleware(next.Object, sessionAffinityConfig, sessionAffinityPolicies);

            var httpContext = new DefaultHttpContext();
            httpContext.SetLoadBalancerFeature(new LoadBalancerFeature());

            await middleware.Invoke(httpContext);

            var contextFeature = httpContext.GetLoadBalancerFeature();
            Assert.Null(contextFeature.SetSessionAffinityKey);
            Assert.Null(contextFeature.SelectedRemoteEndpoint);
        }
    }
}