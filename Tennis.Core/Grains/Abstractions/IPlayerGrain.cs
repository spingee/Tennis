namespace Tennis.Core.Grains.Abstractions;

using System.Collections.Immutable;
using Orleans;

public interface IPlayerGrain : IGrainWithStringKey
{
    Task Create(string matchName, int experience);
}

public record PlayerState(string MatchName, int Experience, ImmutableList<bool> LastPointResults);