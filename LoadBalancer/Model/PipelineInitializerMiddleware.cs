using System;
namespace LoadBalancer.Model
{
	public class PipelineInitializerMiddleware
	{
        private readonly RequestDelegate _next;
		public PipelineInitializerMiddleware(RequestDelegate next)
		{
            _next = next;
		}

        public Task Invoke(HttpContext context)
        {
            context.SetLoadBalancerFeature(new LoadBalancerFeature());

            return _next(context);
        }
    }
}

