using System.Linq;
using System.Reflection;
using Ixfleura.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Ixfleura.Common.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all services marked as an <see cref="IxService"/> to the service collection.
        /// </summary>
        /// <param name="services">
        /// The <see cref="IServiceCollection"/> to add to.
        /// </param>
        /// <returns>
        /// The service collection.
        /// </returns>
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