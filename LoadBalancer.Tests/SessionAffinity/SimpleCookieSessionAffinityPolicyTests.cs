using System.Collections.Generic;
using System.Linq;
using LoadBalancer.Configuration;
using LoadBalancer.Model;
using LoadBalancer.SessionAffinity;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace LoadBalancer.Tests.SessionAffinity
{
    public class SimpleCookieSessionAffinityPolicyTests
    {
        [Fact]
        public void SelectEndpoint_NoAffinityKey_ReturnsNull()
        {
            var sessionAffinityConfig = new Configuration.SessionAffinity { AffinityKeyName = "AffinityKey" };
            var policy = new SimpleCookieSessionAffinityPolicy(sessionAffinityConfig);
            var availableEndpoints = new List<EndpointState>
            {
                new EndpointState("https://test1-endpoint"),
                new EndpointState("https://test2-endpoint")
            };

            var httpContext = new DefaultHttpContext();

            var selectedEndpoint = policy.SelectEndpoint(httpContext, availableEndpoints);

            Assert.Null(selectedEndpoint);
        }

        [Fact]
        public void SelectEndpoint_AffinityKeyInRequestCookie_ReturnsCorrectEndpoint()
        {
            var sessionAffinityConfig = new Configuration.SessionAffinity { AffinityKeyName = "AffinityKey" };
            var policy = new SimpleCookieSessionAffinityPolicy(sessionAffinityConfig);
            var availableEndpoints = new List<EndpointState>
            {
                new EndpointState("https://test1-endpoint"),
                new EndpointState("https://test2-endpoint")
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Cookie"] = "AffinityKey=https%3A%2F%2Ftest1-endpoint";

            var selectedEndpoint = policy.SelectEndpoint(httpContext, availableEndpoints);

            Assert.Equal(availableEndpoints[0], selectedEndpoint);
        }

        [Fact]
        public void AffinitizeResponse_SetsCookieWithEndpointAddress()
        {
            var sessionAffinityConfig = new Configuration.SessionAffinity { AffinityKeyName = "AffinityKey" };
            var policy = new SimpleCookieSessionAffinityPolicy(sessionAffinityConfig);
            var selectedEndpoint = new EndpointState("https://test1-endpoint");

            var httpContext = new DefaultHttpContext();

            policy.AffinitizeResponse(httpContext, selectedEndpoint);

            var cookieValue = httpContext.Response.Headers["Set-Cookie"].FirstOrDefault();
            Assert.NotNull(cookieValue);
            Assert.Contains("AffinityKey=https%3A%2F%2Ftest1-endpoint", cookieValue);
        }
    }
}
