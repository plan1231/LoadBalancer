using System;
using System.Net.Http;
using LoadBalancer.Model;
namespace LoadBalancer.Forwarding
{
	public class ForwardingMiddleware
	{
        private readonly RequestDelegate _next;
        private readonly HttpClient _httpClient;
        public ForwardingMiddleware(RequestDelegate next, HttpClient httpClient)
        {
                    _next = next;
            _httpClient = httpClient;
        }


        public async Task InvokeAsync(HttpContext context)
        {
            ILoadBalancerFeature contextFeature = context.GetLoadBalancerFeature();
            EndpointState? remote = contextFeature.SelectedRemoteEndpoint;
            if (remote == null) throw new InvalidOperationException("No matching remote found, unable to forward request");

            // Call the callback set by the session affinity policy to set the session affinity key. Must do so here
            // before we start writing to the response body
            if(contextFeature.SetSessionAffinityKey != null)
            {
                contextFeature.SetSessionAffinityKey(context, remote);
            }
            // Create a new HttpRequestMessage based on the incoming request
            var requestMessage = new HttpRequestMessage();
            requestMessage.Method = new HttpMethod(context.Request.Method);
            requestMessage.RequestUri = new Uri(remote.Address + context.Request.Path + context.Request.QueryString);

            // Copy the request headers to the new HttpRequestMessage
            foreach (var header in context.Request.Headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            // Forward the request
            HttpResponseMessage responseMessage;
            try
            {
                responseMessage = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync($"Error forwarding request: {e.Message}");
                return;
            }

            // Copy the response status code and headers
            context.Response.StatusCode = (int)responseMessage.StatusCode;
            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            // Copy the response body
            await responseMessage.Content.CopyToAsync(context.Response.Body);
        }
	}
}

