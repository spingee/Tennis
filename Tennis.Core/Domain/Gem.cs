namespace Tennis.Core.Domain;

[GenerateSerializer]
public record Gem
{
    [Id(0)]
    public GemPoint Player1Score { get; private init; } = GemPoint.Zero;

    [Id(1)]
    public GemPoint Player2Score { get; private init; } = GemPoint.Zero;

    [Id(2)]
    public bool IsPlayerOneServe { get; init; } = true;

    [Id(3)]
    public bool IsFinished { get; private set; } = false;

    public bool IsPlayerOneWinner => Player1Score > Player2Score;

    public GemPoint ServePlayerScore => IsPlayerOneServe ? Player1Score : Player2Score;
    public GemPoint OtherPlayerScore => IsPlayerOneServe ? Player2Score : Player1Score;


    public Gem WithServePlayerPlayResult(bool isWon)
    {
        if (IsFinished)
            throw new InvalidOperationException("Gem is already finished");

        bool servePlayerWon = isWon && ((ServePlayerScore == GemPoint.Advance && OtherPlayerScore == GemPoint.Forty)
            || (ServePlayerScore == GemPoint.Forty && OtherPlayerScore < GemPoint.Forty));
        bool otherPlayerWon = !isWon && ((OtherPlayerScore == GemPoint.Advance && ServePlayerScore == GemPoint.Forty)
            || (OtherPlayerScore == GemPoint.Forty && ServePlayerScore < GemPoint.Forty));
        if (servePlayerWon || otherPlayerWon)
            return this with
            {
                IsFinished = true
            };

        if (IsPlayerOneServe)
            return this with
            {
                Player1Score = isWon
                    ? Player2Score == GemPoint.Advance ? Player1Score : Player1Score.Next()
                    : Player1Score == GemPoint.Advance
                        ? Player1Score.LoseAdvantage()
                        : Player1Score,
                Player2Score = isWon
                    ? Player2Score == GemPoint.Advance ? Player2Score.LoseAdvantage() : Player2Score
                    : Player2Score.Next()
            };

        return this with
        {
            Player2Score = isWon
                ? Player1Score == GemPoint.Advance ? Player2Score : Player2Score.Next()
                : Player2Score == GemPoint.Advance
                    ? Player2Score.LoseAdvantage()
                    : Player2Score,
            Player1Score = isWon
                ? Player1Score == GemPoint.Advance ? Player1Score.LoseAdvantage() : Player1Score
                : Player1Score.Next()
        };
    }


    public override string ToString()
    {
        return $"{Player1Score}:{Player2Score}";
    }
}