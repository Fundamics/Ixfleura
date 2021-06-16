using Disqord.Logging;
using Microsoft.Extensions.Logging;

namespace Ixfleura.Services
{
    /// <summary>
    /// Represents a base abstract class which services inherit.
    /// </summary>
    public abstract class IxService : ILogging
    {
        public ILogger Logger { get; }
        
        protected IxService(ILogger logger)
        {
            Logger = logger;
        }
    }
}