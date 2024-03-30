namespace Tennis.Grains;

using Orleans.Runtime;
using Orleans.Streams;
using Tennis.Grains.Abstractions;


public class MatchGrain : Grain, IMatchGrain
{
    private readonly ILogger<MatchGrain> logger;
    private readonly IPersistentState<MatchState> state;

    public MatchGrain(ILogger<MatchGrain> logger,
        [PersistentState(nameof(MatchGrain))]
        IPersistentState<MatchState> state)
    {
        this.logger = logger;
        this.state = state;
    }

    public async Task StartMatch(StartMatchRequest request)
    {
        state.State =  new MatchState(request, new MatchResult(false, Array.Empty<SetResult>()));
        logger.LogInformation(
            "Starting match '{Name}' between players with experience {ExperiencePlayer1} and {ExperiencePlayer2}",
            this.GetPrimaryKeyString(),
            request.ExperiencePlayer1,
            request.ExperiencePlayer2);
        var streamProvider = this.GetStreamProvider(Constants.QueueStreamName);
        var stream = streamProvider.GetStream<int>(StreamId.Create(Constants.PlayerPlayResultStream, this.GetPrimaryKeyString()));
        await stream.SubscribeAsync(PlayerPlayedHandler);
        await streamProvider.GetStream<int>(StreamId.Create(Constants.PlayerPlayRequestStream, this.GetPrimaryKeyString() +
                                "-Player1"))
            .OnNextAsync(0);
        await state.WriteStateAsync();
    }

    public Task<MatchResult> GetResult()
    {
        if (state.State.Result == null)
        {
            DeactivateOnIdle();
            throw new InvalidOperationException("Match has not started yet");
        }
        return Task.FromResult(state.State.Result);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        await state.WriteStateAsync();
        await base.OnDeactivateAsync(reason, cancellationToken);
    }

    private Task PlayerPlayedHandler(int i, StreamSequenceToken token)
    {
        logger.LogInformation("Player {Player} played {Number}", this.GetPrimaryKeyString(), i);
        return Task.CompletedTask;
    }
}