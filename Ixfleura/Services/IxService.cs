using Disqord.Logging;
using Microsoft.Extensions.Logging;

namespace Ixfleura.Services
{
    public abstract class IxService : ILogging
    {
        public ILogger Logger { get; }
        
        protected IxService(ILogger logger)
        {
            Logger = logger;
        }
    }
}