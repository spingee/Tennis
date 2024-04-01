namespace Tennis.Core.Grains;

using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using Tennis.Core.Domain;
using Tennis.Core.Grains.Abstractions;

[ImplicitStreamSubscription(Constants.PlayerPlayResultStream)]
public class MatchGrain : Grain, IMatchGrain
{
    private readonly ILogger logger;
    private readonly IPersistentState<MatchState> state;

    public MatchGrain(
        ILoggerFactory loggerFactory,
        [PersistentState(nameof(MatchGrain))] IPersistentState<MatchState> state)
    {
        this.logger = loggerFactory.CreateLogger(this.GetPrimaryKeyString());
        this.state = state;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("MatchGrain activated {Match}", this.state.State.Match);
        var streamProvider = this.GetStreamProvider(Constants.QueueStreamName);
        var stream =
            streamProvider.GetStream<PlayerPlayResponse>(
                StreamId.Create(Constants.PlayerPlayResultStream, this.GetPrimaryKeyString()));
        var subscriptionHandles = await stream.GetAllSubscriptionHandles();
        if(subscriptionHandles.Count > 0)
        {
            await subscriptionHandles[0].ResumeAsync(PlayerPlayedHandler);
        }
        else
            await stream.SubscribeAsync(PlayerPlayedHandler);
        await base.OnActivateAsync(cancellationToken);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        await state.WriteStateAsync();
        await base.OnDeactivateAsync(reason, cancellationToken);
    }

    public async Task StartMatch(StartMatchRequest request)
    {
        if (state.State.Match != null)
        {
            throw new InvalidOperationException("Match already exists.");
        }
        state.State = new MatchState(request, new Match(this.GetPrimaryKeyString()));
        logger.LogInformation(
            "Starting match '{Name}' between players with experience {ExperiencePlayer1} and {ExperiencePlayer2}",
            this.GetPrimaryKeyString(),
            request.ExperiencePlayer1,
            request.ExperiencePlayer2);
        var matchRegisterGrain = GrainFactory.GetGrain<IMatchRegisterGrain>(0);
        await matchRegisterGrain.RegisterMatch(this.GetPrimaryKeyString());
        await SendPlayerPlayRequest(true);
        await state.WriteStateAsync();
    }

    public Task<Match> GetResult()
    {
        if (state.State.Match == null)
        {
            DeactivateOnIdle();
            throw new InvalidOperationException("Match has not started yet");
        }

        return Task.FromResult(state.State.Match);
    }


    private async Task PlayerPlayedHandler(PlayerPlayResponse response, StreamSequenceToken token)
    {
        state.State = state.State with
        {
            Match = state.State.Match!.WithNextPlayerPlayResult(response.BallWon)
        };

        logger.LogInformation("Player {Player} {Result} the ball. Match score is {Score}",
            response.IsPlayerOne ? "1" : "2",
            response.BallWon ? "won" : "lost", state.State.Match);

        if (!state.State.Match.IsFinished)
        {
            await SendPlayerPlayRequest(state.State.Match!.IsPlayerOnePlayNext);
        }
        else
        {
            logger.LogInformation("Match finished : {Match}", state.State.Match);
            var matchRegisterGrain = GrainFactory.GetGrain<IMatchRegisterGrain>(0);
            await matchRegisterGrain.FinishMatch(this.GetPrimaryKeyString());
            var streamProvider = this.GetStreamProvider(Constants.QueueStreamName);
            var stream =
                streamProvider.GetStream<PlayerPlayResponse>(
                    StreamId.Create(Constants.PlayerPlayResultStream, this.GetPrimaryKeyString()));
            var handles = await stream.GetAllSubscriptionHandles();
            foreach (var handle in handles)
            {
                await handle.UnsubscribeAsync();
            }
            DeactivateOnIdle();
        }

        await state.WriteStateAsync();
    }

    private async Task SendPlayerPlayRequest(bool isPlayerOne)
    {
        var streamProvider = this.GetStreamProvider(Constants.QueueStreamName);
        await streamProvider.GetStream<int>(StreamId.Create(Constants.PlayerPlayRequestStream,
                                this.GetPrimaryKeyString() + (isPlayerOne ? "-Player1" : "-Player2")))
                            .OnNextAsync(0);
    }
}