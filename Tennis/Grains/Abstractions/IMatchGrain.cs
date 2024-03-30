namespace Tennis.Grains.Abstractions;

public interface IMatchGrain : IGrainWithStringKey
{
    Task StartMatch(StartMatchRequest request);
    Task<MatchResult> GetResult();
}

[GenerateSerializer]
public record StartMatchRequest(int ExperiencePlayer1, int ExperiencePlayer2);

[GenerateSerializer]
public record MatchResult(bool IsFinished, IReadOnlyList<SetResult> Sets);

[GenerateSerializer]
public record SetResult(int Player1Score, int Player2Score);

public record MatchState(StartMatchRequest? Request, MatchResult? Result);