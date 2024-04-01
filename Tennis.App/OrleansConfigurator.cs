namespace Tennis.App;

using Tennis.Core;

public static class OrleansConfigurator
{
    public static void UseOrleans(this ConfigureHostBuilder hostBuilder)
    {
        hostBuilder
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
                }
            });
    }
}