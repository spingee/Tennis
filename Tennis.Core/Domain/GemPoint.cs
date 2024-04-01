namespace Tennis.Core.Domain;

[GenerateSerializer]
public record GemPoint : IComparable<GemPoint>
{
    private const int ScoreZeroInt = 0;
    private const int Score15Int = 15;
    private const int Score30Int = 30;
    private const int Score40Int = 40;
    private const int ScoreAdvantageInt = 41;
    private const string ScoreZero = "0";
    private const string Score15 = "15";
    private const string Score30 = "30";
    private const string Score40 = "40";
    private const string ScoreAdvantage = "AD";


    public static readonly GemPoint Zero = new()
        { Value = ScoreZeroInt };

    public static readonly GemPoint Fifteen = new()
        { Value = Score15Int };

    public static readonly GemPoint Thirty = new()
        { Value = Score30Int };

    public static readonly GemPoint Forty = new()
        { Value = Score40Int };

    public static readonly GemPoint Advance = new()
        { Value = ScoreAdvantageInt };

    [Id(0)]
    public int Value { get; private init; } = 0;


    public GemPoint Next() =>
        this with
        {
            Value = Value switch
            {
                ScoreZeroInt => Score15Int,
                Score15Int => Score30Int,
                Score30Int => Score40Int,
                Score40Int => ScoreAdvantageInt,
                _ => throw new ArgumentOutOfRangeException()
            }
        };

    public GemPoint LoseAdvantage()
    {
        if (Value != ScoreAdvantageInt)
            throw new InvalidOperationException();
        return this with
        {
            Value = Value switch
            {
                ScoreAdvantageInt => Score40Int,
                _ => throw new InvalidOperationException()
            }
        };
    }

    public int CompareTo(GemPoint? other)
    {
        if (ReferenceEquals(this, other))
            return 0;
        if (ReferenceEquals(null, other))
            return 1;
        return Value.CompareTo(other.Value);
    }

    public static bool operator <(GemPoint? left, GemPoint? right)
    {
        return Comparer<GemPoint>.Default.Compare(left, right) < 0;
    }

    public static bool operator >(GemPoint? left, GemPoint? right)
    {
        return Comparer<GemPoint>.Default.Compare(left, right) > 0;
    }

    public static bool operator <=(GemPoint? left, GemPoint? right)
    {
        return Comparer<GemPoint>.Default.Compare(left, right) <= 0;
    }

    public static bool operator >=(GemPoint? left, GemPoint? right)
    {
        return Comparer<GemPoint>.Default.Compare(left, right) >= 0;
    }

    public override string ToString()
    {
        return Value switch
        {
            ScoreZeroInt => ScoreZero,
            Score15Int => Score15,
            Score30Int => Score30,
            Score40Int => Score40,
            ScoreAdvantageInt => ScoreAdvantage,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}