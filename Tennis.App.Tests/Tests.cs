namespace Tennis.App.Tests;

using System.Net.Http.Json;
using System.Text;
using Tennis.App.Contract;
using Xunit;

public class Tests : IClassFixture<Fixture>
{
    private readonly Fixture fixture;

    public Tests(Fixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact(Timeout = 20000)]
    public async Task End2EndTest()
    {
        var response = await fixture.HttpClient.PostAsync("/match/match1"
            , new StringContent("""
                            {
                              "experiencePlayer1": 60,
                              "experiencePlayer2": 77
                            }
                            """, Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();

        MatchResult? result;
        do
        {
            result = await fixture.HttpClient.GetFromJsonAsync<MatchResult>("/match/match1");
            Assert.NotNull(result);
        }
        while (!result.Sets.Any(f=> f.Player1 != 0 || f.Player2 != 0));
        Assert.NotEqual(new SetScore(0,0), result.Sets.First());
    }
}