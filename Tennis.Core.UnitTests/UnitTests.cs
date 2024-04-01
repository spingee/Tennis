namespace Tennis.Core.UnitTests;

using FluentAssertions;
using Tennis.Core.Domain;

public class UnitTests
{
    [Fact(Timeout = 1000)]
    public void MatchResult_ShouldFinishSuccessfullyWith3Sets()
    {
        var matchResult = new Match("Test match");

        while (!matchResult.IsFinished)
        {
            matchResult = matchResult.WithNextPlayerPlayResult(Random.Shared.Next(0, 2) == 1);
        }

        matchResult.Sets.Should().HaveCount(3);

        matchResult.Sets
                   .SelectMany(f => f.Gems)
                   .Select(f => f.Player1Score.Value)
                   .Where(f => f > 0)
                   .Should().HaveCountGreaterThan(0);

        matchResult.Sets
                   .SelectMany(f => f.Gems)
                   .Select(f => f.Player2Score.Value)
                   .Where(f => f > 0)
                   .Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void GemPointComparerTests()
    {
        GemPoint.Advance.Should().BeGreaterThan(GemPoint.Forty);
    }

    [Fact]
    public void GemTest()
    {
        var gem = new Gem { IsPlayerOneServe = true };
        gem.Player1Score.Should().Be(GemPoint.Zero);
        Gem result = gem.WithServePlayerPlayResult(true);
        result.Player1Score.Should().Be(GemPoint.Fifteen);
        result = result.WithServePlayerPlayResult(true);
        result.Player1Score.Should().Be(GemPoint.Thirty);
        result = result.WithServePlayerPlayResult(true);
        result.Player1Score.Should().Be(GemPoint.Forty);
        result = result.WithServePlayerPlayResult(true);
        result.IsFinished.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GemTest_Advantage(bool isPlayerOneServe)
    {
        var gem = new Gem { IsPlayerOneServe = isPlayerOneServe };

        Gem result = gem.WithServePlayerPlayResult(true);
        result = result.WithServePlayerPlayResult(true);
        result = result.WithServePlayerPlayResult(true);
        result = result.WithServePlayerPlayResult(false);
        result = result.WithServePlayerPlayResult(false);
        result = result.WithServePlayerPlayResult(false);
        result = result.WithServePlayerPlayResult(false);
        result.ServePlayerScore.Should().Be(GemPoint.Forty);
        result.OtherPlayerScore.Should().Be(GemPoint.Advance);
        result.IsFinished.Should().BeFalse();
        result = result.WithServePlayerPlayResult(true);
        result = result.WithServePlayerPlayResult(true);
        result.ServePlayerScore.Should().Be(GemPoint.Advance);
        result.OtherPlayerScore.Should().Be(GemPoint.Forty);
        result = result.WithServePlayerPlayResult(true);
        result.IsFinished.Should().BeTrue();
        result.IsPlayerOneWinner.Should().Be(isPlayerOneServe);
    }
}