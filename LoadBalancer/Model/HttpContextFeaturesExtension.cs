using System;
namespace LoadBalancer.Model
{
	public static class HttpContextFeaturesExtension
	{
		public static ILoadBalancerFeature GetLoadBalancerFeature(this HttpContext context)
		{
            return context.Features.Get<ILoadBalancerFeature>() ?? throw new InvalidOperationException($"{typeof(ILoadBalancerFeature).FullName} is missing.");
        }

        public static void SetLoadBalancerFeature(this HttpContext context, ILoadBalancerFeature feature)
        {
            context.Features.Set<ILoadBalancerFeature>(feature);
        }
    }


}

