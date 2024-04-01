namespace Tennis.App.Contract;

using Tennis.Core.Domain;

public record MatchResult(bool IsFinished, IReadOnlyList<SetScore> Sets);

public record SetScore(int Player1, int Player2)
{
    public static SetScore FromSet(Set set)
    {
        return new SetScore(set.Player1Score, set.Player2Score);
    }
}