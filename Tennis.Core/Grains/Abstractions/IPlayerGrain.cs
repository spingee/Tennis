namespace Tennis.Core.Grains.Abstractions;

using Orleans;

public interface IPlayerGrain : IGrainWithStringKey
{
    Task Create(string matchName, int experience);
}