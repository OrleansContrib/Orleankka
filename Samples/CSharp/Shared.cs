using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Orleans.Hosting;

namespace Orleankka
{
    static class DemoExtensions
    {
        public static async Task<IHost> StartServer(this IHostBuilder builder)
        {
            return await builder
                .UseOrleans(c => c
                    .UseLocalhostClustering()
                    .AddMemoryGrainStorageAsDefault()
                    .AddMemoryGrainStorage("PubSubStore")
                    .AddMemoryStreams("sms")
                    .UseInMemoryReminderService())
                .StartAsync();
        }
    }
}