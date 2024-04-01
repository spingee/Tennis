namespace Tennis.Core.Domain;

using System.Collections.Immutable;

[GenerateSerializer]
public record Set
{
    public bool IsFinished => (Math.Abs(Player1Score - Player2Score) > 1 && Math.Max(Player1Score, Player2Score) > 5)
        || (Math.Abs(Player1Score - Player2Score) == 1 && Math.Max(Player1Score, Player2Score) == 7);

    public bool IsTieBreak => Player1Score == 6 && Player2Score == 6;

    [Id(0)]
    public ImmutableList<Gem> Gems { get; private init; } = ImmutableList<Gem>.Empty;

    [Id(1)]
    public int Player1Score { get; private init; } = 0;

    [Id(2)]
    public int Player2Score { get; private init; } = 0;

    [Id(3)]
    public bool IsPlayerOnePlayNext { get; private init; } = true;

    public bool IsPlayerOneWinner => Player1Score > Player2Score;

    public Set WithNextPlayerPlayResult(bool isWon)
    {
        if (IsFinished)
            throw new InvalidOperationException("Set is already finished");

        var currentGem = Gems.FirstOrDefault(f => !f.IsFinished);
        if (currentGem == null)
        {
            var lastFinishedGem = Gems.LastOrDefault();
            currentGem = new Gem { IsPlayerOneServe = !lastFinishedGem?.IsPlayerOneServe ?? true };
        }

        var newResult = currentGem.WithServePlayerPlayResult(isWon);
        var immutableList = Gems.Remove(currentGem);
        if (newResult.IsFinished)
        {
            return this with
            {
                Gems = immutableList.Add(newResult),
                Player1Score = newResult.IsPlayerOneWinner ? Player1Score + 1 : Player1Score,
                Player2Score = newResult.IsPlayerOneWinner ? Player2Score : Player2Score + 1,
                IsPlayerOnePlayNext = !IsPlayerOnePlayNext
            };
        }

        return this with { Gems = immutableList.Add(newResult), IsPlayerOnePlayNext = newResult.IsPlayerOneServe};
    }

    public override string ToString()
    {
        return $"{Player1Score}-{Player2Score}";
    }
}