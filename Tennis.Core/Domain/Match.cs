namespace Tennis.Core.Domain;

using System.Collections.Immutable;

[GenerateSerializer]
public record Match(string Name)
{
    public bool IsFinished => Sets.Count == 3 && Sets[^1].IsFinished;

    public bool IsPlayerOnePlayNext => Sets.LastOrDefault()?.IsPlayerOnePlayNext ?? true;

    [Id(99)]
    public ImmutableList<Set> Sets { get; private init; } = new [] { new Set() }.ToImmutableList();



    public Match WithNextPlayerPlayResult(bool isWon)
    {
        if (IsFinished)
            throw new InvalidOperationException("Match is already finished");
        var setResult = Sets.First(f => !f.IsFinished);
        var newResult = setResult.WithNextPlayerPlayResult(isWon);
        var immutableList = Sets.Remove(setResult);
        if (newResult.IsFinished)
        {
            return this with
            {
                Sets = immutableList.Add(newResult)
            };
        }

        return this with { Sets = immutableList.Add(newResult) };
    }

    public override string ToString()
    {
        return string.Join(", ", Sets.Select(f => f.ToString()));
    }
}