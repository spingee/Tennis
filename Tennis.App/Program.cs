using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Tennis.App.Contract;
using Tennis.Core;
using Tennis.Core.Grains.Abstractions;

var hostBuilder = WebApplication.CreateBuilder(args);

hostBuilder.Services.AddLogging(builder => builder.AddSystemdConsole(options =>
{
    options.UseUtcTimestamp = false;
    options.TimestampFormat = $"{CultureInfo.InvariantCulture.DateTimeFormat.UniversalSortableDateTimePattern}: ";
}));


hostBuilder
    .Host
    .UseOrleans((host, builder) =>
    {
        var configuration = host.Configuration;
        var inMemory = configuration.GetValue<bool>("Orleans:InMemory", false);

        builder.UseLocalhostClustering();
        if (!inMemory)
        {
            var tableConnString = configuration.GetValue<string>("Orleans:AzureTableStorage:ConnectionString")!;
            var queueConnString = configuration.GetValue<string>("Orleans:AzureQueue:ConnectionString")!;
            builder.AddAzureTableGrainStorageAsDefault(options =>
            {
                options.TableName = "GrainState";
                options.ConfigureTableServiceClient(tableConnString);
            });
            builder.AddAzureQueueStreams(Constants.QueueStreamName,
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
                               options.GetQueueMsgsTimerPeriod = TimeSpan.FromMilliseconds(50);
                           }));
                       })
                   .AddAzureTableGrainStorage("PubSubStore",
                       options =>
                       {
                           options.ConfigureTableServiceClient(tableConnString);
                           options.TableName = "PubSubStore";
                       });

        }
        else
        {
            builder.AddMemoryGrainStorageAsDefault();
            builder.AddMemoryGrainStorage("PubSubStore");
            builder.AddMemoryStreams(Constants.QueueStreamName, configurator => configurator.ConfigurePullingAgent(ob => ob.Configure(options =>
            {
                options.GetQueueMsgsTimerPeriod = TimeSpan.FromMilliseconds(1);
            })));
        }

    });


var app = hostBuilder.Build();

app.MapGet("/match/{name}", async (string name, IGrainFactory factory) =>
{
    var matchGrain = factory.GetGrain<IMatchGrain>(name);
    var result = await matchGrain.GetResult();
    return new MatchResult(result.IsFinished, result.Sets.Select(SetScore.FromSet).ToList());
});

app.MapPost("/match/{name}", async (string name, [FromBody]StartMatchRequest request, IGrainFactory factory) =>
{
    await factory.GetGrain<IPlayerGrain>(name + "-Player1").Create(name, request.ExperiencePlayer1);
    await factory.GetGrain<IPlayerGrain>(name + "-Player2").Create(name, request.ExperiencePlayer2);
    var matchGrain = factory.GetGrain<IMatchGrain>(name);
    await matchGrain.StartMatch(request);
});

app.Run();



//needed for tests
public partial class Program { }