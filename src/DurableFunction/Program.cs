using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(c =>
    {
    })
    .ConfigureServices(service =>
    {
        service
            .AddLogging();
    })
.Build();
await host.RunAsync();
