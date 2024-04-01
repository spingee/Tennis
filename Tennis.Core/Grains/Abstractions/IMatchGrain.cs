namespace Tennis.Core.Grains.Abstractions;

using System.Collections.Immutable;
using Orleans;
using Tennis.Core.Domain;

public interface IMatchGrain : IGrainWithStringKey
{
    Task StartMatch(StartMatchRequest request);
    Task<Match> GetResult();
}

[GenerateSerializer]
public record StartMatchRequest(int ExperiencePlayer1, int ExperiencePlayer2);

[GenerateSerializer]
public record PlayerPlayResponse(bool BallWon, bool IsPlayerOne);

public record MatchState(StartMatchRequest? Request, Match? Match);

public record PlayerState(string MatchName, int Experience, ImmutableList<bool> LastPointResults);