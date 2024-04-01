namespace Tennis.Core.Grains;

using System.Collections.Immutable;
using Orleans.Runtime;
using Orleans.Streams;
using Tennis.Core.Grains.Abstractions;

[ImplicitStreamSubscription(Constants.PlayerPlayRequestStream)]
public class PlayerGrain : Grain, IPlayerGrain
{
    private readonly IPersistentState<PlayerState> state;

    public PlayerGrain(
        [PersistentState(nameof(PlayerGrain))]
        IPersistentState<PlayerState> state)
    {
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
        var streamProvider = this.GetStreamProvider(Constants.QueueStreamName);
        var lastPointResults = state.State.LastPointResults;
        int next = Random.Shared.Next(state.State.Experience, 100);
        var result = next > (lastPointResults.TakeLast(5).Count(f => f) > 4 ? 90 : 70);

        state.State = state.State with
        {
            LastPointResults = state.State.LastPointResults.Add(result)
        };
        return streamProvider.GetStream<PlayerPlayResponse>(StreamId.Create(Constants.PlayerPlayResultStream,
                                 state.State.MatchName))
                             .OnNextAsync(new PlayerPlayResponse(result, this.GetPrimaryKeyString().Contains("Player1")));
    }

    public Task Create(string matchName, int experience)
    {
        state.State = new PlayerState(matchName, experience, ImmutableList<bool>.Empty);
        return state.WriteStateAsync();
    }
}