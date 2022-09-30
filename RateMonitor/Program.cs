using RateMonitor;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services )=>
    {
        services.RegisterJob(context.Configuration);
    })
    .Build();

await host.RunAsync();
