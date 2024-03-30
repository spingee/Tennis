using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Tennis.Grains.Abstractions;

var hostBuilder = WebApplication.CreateBuilder(args);

hostBuilder.Services.AddLogging(builder => builder.AddSystemdConsole(options =>
{
    options.UseUtcTimestamp = false;
    options.TimestampFormat = $"{CultureInfo.InvariantCulture.DateTimeFormat.UniversalSortableDateTimePattern}: ";
}));


var configuration = hostBuilder.Configuration;

hostBuilder
    .Host
    .UseOrleans(builder =>
    {
        var tableConnString = configuration.GetValue<string>("Orleans:AzureTableStorage:ConnectionString")!;
        var queueConnString = configuration.GetValue<string>("Orleans:AzureQueue:ConnectionString")!;
        builder.AddAzureTableGrainStorageAsDefault(options =>
        {
            options.TableName = "GrainState";
            options.ConfigureTableServiceClient(tableConnString);
        });
        builder.AddAzureQueueStreams("QueueProvider",
                   configurator =>
                   {
                       configurator.ConfigureAzureQueue(
                           ob => ob.Configure(options =>
                           {
                               options.ConfigureQueueServiceClient(queueConnString);
                               options.QueueNames = new List<string>
                                   { "yourprefix-azurequeueprovider-0" };
                           }));
                       configurator.ConfigureCacheSize(1024);
                       configurator.ConfigurePullingAgent(ob => ob.Configure(options =>
                       {
                           options.GetQueueMsgsTimerPeriod = TimeSpan.FromMilliseconds(200);
                       }));
                   })
               .AddAzureTableGrainStorage("PubSubStore",
                   options =>
                   {
                       options.ConfigureTableServiceClient(tableConnString);
                       options.TableName = "PubSubStore";
                   });
        builder.UseLocalhostClustering();
    });


var app = hostBuilder.Build();

app.MapGet("/match/{name}", (string name, IGrainFactory factory) =>
{
    var matchGrain = factory.GetGrain<IMatchGrain>(name);
    return matchGrain.GetResult();
});

app.MapPut("/match/{name}", (string name, [FromBody]StartMatchRequest request, IGrainFactory factory) =>
{
    var matchGrain = factory.GetGrain<IMatchGrain>(name);
    return matchGrain.StartMatch(request);
});



app.Run();