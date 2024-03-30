namespace Tennis.Grains;

using Orleans.Providers;
using Orleans.Runtime;
using Tennis.Grains.Abstractions;

public class MatchGrain : Grain, IMatchGrain
{
    private readonly ILogger<MatchGrain> logger;
    private readonly IPersistentState<MatchState> state;

    public MatchGrain(ILogger<MatchGrain> logger,
        [PersistentState(nameof(MatchGrain), ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)]
        IPersistentState<MatchState> state)
    {
        this.logger = logger;
        this.state = state;
    }

    public Task StartMatch(StartMatchRequest request)
    {
        state.State =  new MatchState(request, new MatchResult(false, Array.Empty<SetResult>()));
        logger.LogInformation(
            "Starting match '{Name}' between players with experience {ExperiencePlayer1} and {ExperiencePlayer2}",
            this.GetPrimaryKeyString(),
            request.ExperiencePlayer1,
            request.ExperiencePlayer2);
        return state.WriteStateAsync();
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
}