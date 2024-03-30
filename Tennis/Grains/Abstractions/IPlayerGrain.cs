namespace Tennis.Grains.Abstractions;

public interface IPlayerGrain : IGrainWithStringKey
{
    Task Create(string matchName, int experience);
}