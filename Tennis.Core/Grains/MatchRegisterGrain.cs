namespace Tennis.Core.Grains;

using Orleans.Runtime;
using Tennis.Core.Grains.Abstractions;

[KeepAlive]
public class MatchRegisterGrain : Grain, IMatchRegisterGrain
{
    private readonly IPersistentState<MatchRegisterState> state;

    public MatchRegisterGrain([PersistentState(nameof(MatchRegisterGrain))] IPersistentState<MatchRegisterState> state)
    {
        this.state = state;
    }

    public Task RegisterMatch(string name)
    {
        state.State.RunningMatches.Add(name);
        return state.WriteStateAsync();
    }

    public Task FinishMatch(string name)
    {
        state.State.RunningMatches.Remove(name);
        return state.WriteStateAsync();
    }
}