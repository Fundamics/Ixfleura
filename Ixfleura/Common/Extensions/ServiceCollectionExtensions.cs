using System.Linq;
using System.Reflection;
using Ixfleura.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Ixfleura.Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIxServices(this IServiceCollection services)
        {
            var baseType = typeof(IxService);
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsAssignableTo(baseType) && !x.IsAbstract);

            foreach (var type in types)
                services.AddSingleton(type);

            return services;
        }
    }
}