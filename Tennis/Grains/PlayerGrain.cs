namespace Tennis.Grains;

using Orleans.Runtime;
using Orleans.Streams;
using Tennis.Grains.Abstractions;

[ImplicitStreamSubscription(Constants.PlayerPlayRequestStream)]
public class PlayerGrain : Grain, IPlayerGrain
{
    private readonly ILogger<PlayerGrain> logger;
    private readonly IPersistentState<PlayerState> state;

    public PlayerGrain(ILogger<PlayerGrain> logger,
        [PersistentState(nameof(PlayerGrain))]
        IPersistentState<PlayerState> state)
    {
        this.logger = logger;
        this.state = state;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = this.GetStreamProvider(Constants.QueueStreamName);
        var stream =
            streamProvider.GetStream<int>(
                StreamId.Create(Constants.PlayerPlayRequestStream, this.GetPrimaryKeyString()));
        await stream.SubscribeAsync(PlayHandler);
        await base.OnActivateAsync(cancellationToken);
    }

    private Task PlayHandler(int i, StreamSequenceToken token)
    {
        logger.LogInformation("Player {Player} played {Number}", this.GetPrimaryKeyString(), i);
        var streamProvider = this.GetStreamProvider(Constants.QueueStreamName);
        return streamProvider.GetStream<int>(StreamId.Create(Constants.PlayerPlayResultStream,
                                 state.State.MatchName))
                             .OnNextAsync(0);
    }

    public Task Create(string matchName, int experience)
    {
        state.State = new PlayerState(matchName, experience);
        return state.WriteStateAsync();
    }
}