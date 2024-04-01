namespace Tennis.Core.Grains.Abstractions;

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

public record MatchState()
{
    public StartMatchRequest? Request { get; init; }
    public MatchStorage? Match { get; set; }
}

public record MatchStorage
{
    public string Name { get; set; } = string.Empty;
    public List<SetStorage> Sets { get; set; } = new List<SetStorage>();

    public static MatchStorage FromMatch(Match match)
    {
        var sets = match.Sets.Select(f => new SetStorage
        {
            Gems = f.Gems.Select(g => new GemStorage
            {
                Player1Score = g.Player1Score.Value,
                Player2Score = g.Player2Score.Value,
                IsPlayerOneServe = g.IsPlayerOneServe,
                IsFinished = g.IsFinished
            }).ToList(),
            Player1Score = f.Player1Score,
            Player2Score = f.Player2Score,
            IsPlayerOnePlayNext = f.IsPlayerOnePlayNext
        }).ToList();
        return new MatchStorage { Sets = sets };
    }
}

public record SetStorage
{
    public List<GemStorage> Gems { get; set; } = new List<GemStorage>();
    public int Player1Score { get; set; }
    public int Player2Score { get; set; }
    public bool IsPlayerOnePlayNext { get; set; }
}

public record GemStorage
{
    public int Player1Score { get; set; }
    public int Player2Score { get; set; }
    public bool IsPlayerOneServe { get; set; }
    public bool IsFinished { get; set; }
}