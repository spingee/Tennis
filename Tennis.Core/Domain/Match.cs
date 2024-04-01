namespace Tennis.Core.Domain;

using System.Collections.Immutable;
using Tennis.Core.Grains.Abstractions;

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
        var setResult = Sets.FirstOrDefault(f => !f.IsFinished, new Set());
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

    public static Match FromStorage(MatchStorage storage)
    {
        return new Match(storage.Name)
        {
            Sets = storage.Sets.Select(Set.FromStorage).ToImmutableList()
        };
    }

    public override string ToString()
    {
        return string.Join(", ", Sets.Select(f => f.ToString()));
    }
}