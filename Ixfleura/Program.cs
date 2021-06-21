using System;
using System.Collections.Generic;
using System.Linq;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Ixfleura.Common.Configuration;
using Ixfleura.Common.Extensions;
using Ixfleura.Common.Globals;
using Ixfleura.Data;
using Ixfleura.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Ixfleura
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(x =>
                {
                    x.AddCommandLine(args);
                    x.AddJsonFile("config.json");
                })
                .ConfigureLogging(x =>
                {
                    var logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",theme: SystemConsoleTheme.Grayscale)
                        .CreateLogger();
                    x.AddSerilog(logger, true);

                    x.Services.Remove(x.Services.First(serviceDescriptor => serviceDescriptor.ServiceType == typeof(ILogger<>)));
                    x.Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                })
                .ConfigureDiscordBot((context, bot) =>
                {
                    bot.Token = context.Configuration["discord:token"];
                    bot.Intents = GatewayIntents.All;
                    bot.UseMentionPrefix = true;
                    bot.OwnerIds = new[] {IxfleuraGlobals.OwnerId};
                    bot.Prefixes = context.Configuration.GetSection("discord:prefixes").Get<string[]>();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddHttpClient<SearchService>();

                    services.AddDbContextFactory<IxfleuraDbContext>(options =>
                    {
                        options.UseNpgsql(context.Configuration["database:connection"]);
                        options.UseSnakeCaseNamingConvention();
                    });
                    
                    services.Configure<CampaignsConfiguration>(options =>
                    {
                        var section = context.Configuration.GetSection("campaigns");
                        var typeSections = section.GetSection("types").GetChildren();
                        
                        options.ChannelId = section.GetValue<ulong>("channel_id");
                        options.LogChannelId = section.GetValue<ulong>("log_channel_id");
                        options.Types = new List<CampaignTypeConfiguration>();

                        foreach (var typeSection in typeSections)
                        {
                            options.Types.Add(new CampaignTypeConfiguration
                            {
                                Name = typeSection.GetValue<string>("name"),
                                Enabled = typeSection.GetValue<bool>("enabled"),
                                RoleIds = typeSection.GetSection("role_ids").GetChildren().Select(x => x.Get<ulong>()).ToArray(),
                                RequiredRoleIds = typeSection.GetSection("required_role_ids").GetChildren().Select(x => x.Get<ulong>()).ToArray(),
                                Duration = typeSection.GetValue<TimeSpan>("duration"),
                                MinimumVotes = typeSection.GetValue<int>("minimum_votes"),
                                MinimumRatio = typeSection.GetValue<decimal>("minimum_ratio")
                            });
                        }
                    });
                    
                    services
                        .AddSingleton<Random>()
                        .AddIxServices();
                })
                .Build();

            try
            {
                host.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }
    }
}
