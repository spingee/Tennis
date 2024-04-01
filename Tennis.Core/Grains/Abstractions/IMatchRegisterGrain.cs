namespace Tennis.Core.Grains.Abstractions;

using System.Collections.Immutable;

public interface IMatchRegisterGrain : IGrainWithIntegerKey
{
    Task RegisterMatch(string name);
    Task FinishMatch(string name);
}

public record MatchRegisterState
{
    public IList<string> RunningMatches { get; init; } = new List<string>();
}