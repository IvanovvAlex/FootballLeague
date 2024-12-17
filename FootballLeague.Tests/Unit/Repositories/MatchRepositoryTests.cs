using AutoFixture;
using FluentAssertions;
using FootballLeague.Data;
using FootballLeague.Data.Entities;
using FootballLeague.Data.Interfaces;
using FootballLeague.Data.Repositories;
using FootballLeague.Tests.Utils;
using Microsoft.EntityFrameworkCore;
using Moq;
using Match = FootballLeague.Data.Entities.Match;

namespace FootballLeague.Tests.Unit.Repositories
{
    public class MatchRepositoryTests : IDisposable
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly AppDbContext _context;
        private readonly MatchRepository _matchRepository;
        private readonly Mock<ITeamRepository> _teamRepositoryMock;

        public MatchRepositoryTests()
        {
            _fixture.Customize(new ActiveCustomization());
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
           .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            DbContextOptions<AppDbContext>? options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"MatchesTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new(options);
            _teamRepositoryMock = new Mock<ITeamRepository>();
            _matchRepository = new(_context, _teamRepositoryMock.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldAddMatch_WhenTeamsExist()
        {
            // Arrange
            Team homeTeam = _fixture.Build<Team>().Create();
            Team awayTeam = _fixture.Build<Team>().Create();
            Match match = _fixture.Build<Match>()
                .With(m => m.HomeTeamId, homeTeam.Id)
                .With(m => m.AwayTeamId, awayTeam.Id)
                .Create();

            await _context.Teams.AddRangeAsync(homeTeam, awayTeam);
            await _context.SaveChangesAsync();

            // Act
            Match? result = await _matchRepository.AddAsync(match);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(match, options => options.Excluding(m => m.HomeTeam).Excluding(m => m.AwayTeam));
            _context.Matches.Should().Contain(m => m.Id == match.Id);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnNull_WhenTeamsDoNotExist()
        {
            // Arrange
            Match match = _fixture.Create<Match>();

            // Act
            Match? result = await _matchRepository.AddAsync(match);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnNonDeletedMatches()
        {
            // Arrange
            List<Match> matches = _fixture.Build<Match>()
                .With(m => m.IsDeleted, false)
                .CreateMany(3)
                .ToList();

            List<Match> deletedMatches = _fixture.Build<Match>()
                .With(m => m.IsDeleted, true)
                .CreateMany(2)
                .ToList();

            await _context.Matches.AddRangeAsync(matches.Concat(deletedMatches));
            await _context.SaveChangesAsync();

            // Act
            IEnumerable<Match> result = await _matchRepository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().OnlyContain(m => m.IsDeleted == false);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectMatch()
        {
            // Arrange
            Match match = _fixture.Build<Match>()
                .With(m => m.IsDeleted, false)
                .Create();
            await _context.Matches.AddAsync(match);
            await _context.SaveChangesAsync();

            // Act
            Match? result = await _matchRepository.GetByIdAsync(match.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(match);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenMatchIsDeleted()
        {
            // Arrange
            Match match = _fixture.Build<Match>()
                .With(m => m.IsDeleted, true)
                .Create();
            await _context.Matches.AddAsync(match);
            await _context.SaveChangesAsync();

            // Act
            Match? result = await _matchRepository.GetByIdAsync(match.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateMatchAndAdjustRank()
        {
            // Arrange
            Team homeTeam = _fixture.Build<Team>()
                .With(t => t.Id, Guid.NewGuid())
                .Without(t => t.HomeMatches)
                .Without(t => t.AwayMatches)
                .Create();

            Team awayTeam = _fixture.Build<Team>()
                .With(t => t.Id, Guid.NewGuid())
                .Without(t => t.HomeMatches)
                .Without(t => t.AwayMatches)
                .Create();

            Match oldMatch = _fixture.Build<Match>()
                .With(m => m.Id, Guid.NewGuid())
                .With(m => m.HomeTeamId, homeTeam.Id)
                .With(m => m.AwayTeamId, awayTeam.Id)
                .With(m => m.IsDeleted, false)
                .Without(m => m.HomeTeam)
                .Without(m => m.AwayTeam)
                .Create();

            await _context.Teams.AddRangeAsync(homeTeam, awayTeam);
            await _context.Matches.AddAsync(oldMatch);
            await _context.SaveChangesAsync();

            Match updatedMatch = _fixture.Build<Match>()
                .With(m => m.Id, oldMatch.Id)
                .With(m => m.HomeTeamId, homeTeam.Id)
                .With(m => m.AwayTeamId, awayTeam.Id)
                .With(m => m.HomeTeamScore, 3)
                .With(m => m.AwayTeamScore, 1)
                .With(m => m.IsDeleted, false)
                .Without(m => m.HomeTeam)
                .Without(m => m.AwayTeam)
                .Create();

            _teamRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Team>()))
                .Returns((Team team) => new ValueTask<Team>(team));

            // Act
            Match? result = await _matchRepository.UpdateAsync(updatedMatch);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(updatedMatch, options => options
                .Excluding(m => m.ModifiedOn)
                .Excluding(m => m.CreatedOn)
                .Excluding(m => m.HomeTeam)
                .Excluding(m => m.AwayTeam)
                .Excluding(m => m.IsDeleted));
        }

        [Fact]
        public async Task DeleteAsync_ShouldMarkMatchAsDeletedAndAdjustRank()
        {
            // Arrange
            Team homeTeam = _fixture.Build<Team>().Create();
            Team awayTeam = _fixture.Build<Team>().Create();
            Match match = _fixture.Build<Match>()
                .With(m => m.HomeTeamId, homeTeam.Id)
                .With(m => m.AwayTeamId, awayTeam.Id)
                .With(m => m.IsDeleted, false)
                .Create();

            await _context.Teams.AddRangeAsync(homeTeam, awayTeam);
            await _context.Matches.AddAsync(match);
            await _context.SaveChangesAsync();

            _teamRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Team>()))
                .Returns((Team team) => new ValueTask<Team>(team));
            // Act
            bool result = await _matchRepository.DeleteAsync(match.Id);

            // Assert
            result.Should().BeTrue();
            _context.Matches.FirstOrDefault(m => m.Id == match.Id)?.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenMatchDoesNotExist()
        {
            // Arrange
            Guid nonExistingMatchId = Guid.NewGuid();

            // Act
            bool result = await _matchRepository.DeleteAsync(nonExistingMatchId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task AddAsync_ShouldReturnNull_WhenHomeOrAwayTeamIsNull()
        {
            // Arrange
            Guid invalidHomeTeamId = Guid.NewGuid();
            Guid invalidAwayTeamId = Guid.NewGuid();
            Match match = _fixture.Build<Match>()
                .With(m => m.HomeTeamId, invalidHomeTeamId)
                .With(m => m.AwayTeamId, invalidAwayTeamId)
                .Create();

            // Act
            Match? result = await _matchRepository.AddAsync(match);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenMatchDoesNotExist()
        {
            // Arrange
            Match nonExistingMatch = _fixture.Build<Match>()
                .With(m => m.Id, Guid.NewGuid())
                .Create();

            // Act
            Match? result = await _matchRepository.UpdateAsync(nonExistingMatch);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_ShouldNotAdjustRank_WhenMatchScoresAreUnchanged()
        {
            // Arrange
            Team homeTeam = _fixture.Build<Team>().With(t => t.Rank, 5).Create();
            Team awayTeam = _fixture.Build<Team>().With(t => t.Rank, 3).Create();
            Match match = _fixture.Build<Match>()
                .With(m => m.HomeTeamId, homeTeam.Id)
                .With(m => m.AwayTeamId, awayTeam.Id)
                .With(m => m.HomeTeamScore, 2)
                .With(m => m.AwayTeamScore, 2)
                .Create();

            await _context.Teams.AddRangeAsync(homeTeam, awayTeam);
            await _context.Matches.AddAsync(match);
            await _context.SaveChangesAsync();

            Match updatedMatch = _fixture.Build<Match>()
                .With(m => m.Id, match.Id)
                .With(m => m.HomeTeamId, homeTeam.Id)
                .With(m => m.AwayTeamId, awayTeam.Id)
                .With(m => m.HomeTeamScore, 2) // Same score
                .With(m => m.AwayTeamScore, 2) // Same score
                .Create();

            _teamRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Team>()))
                .Returns((Team team) => new ValueTask<Team>(team));

            // Act
            Match? result = await _matchRepository.UpdateAsync(updatedMatch);

            // Assert
            result.Should().NotBeNull();
            homeTeam.Rank.Should().Be(5);
            awayTeam.Rank.Should().Be(3);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenMatchIsAlreadyDeleted()
        {
            // Arrange
            Match deletedMatch = _fixture.Build<Match>()
                .With(m => m.IsDeleted, true)
                .Create();

            await _context.Matches.AddAsync(deletedMatch);
            await _context.SaveChangesAsync();

            // Act
            bool result = await _matchRepository.DeleteAsync(deletedMatch.Id);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task AddAsync_ShouldNotUpdateRank_WhenMatchIsInFuture()
        {
            // Arrange
            Team homeTeam = _fixture.Build<Team>().With(t => t.Rank, 5).Create();
            Team awayTeam = _fixture.Build<Team>().With(t => t.Rank, 3).Create();

            Match futureMatch = _fixture.Build<Match>()
                .With(m => m.HomeTeamId, homeTeam.Id)
                .With(m => m.AwayTeamId, awayTeam.Id)
                .With(m => m.StartTime, DateTime.Now.AddDays(1)) // Future time
                .With(m => m.EndTime, DateTime.Now.AddDays(2))
                .With(m => m.HomeTeamScore, 2)
                .With(m => m.AwayTeamScore, 1)
                .Create();

            await _context.Teams.AddRangeAsync(homeTeam, awayTeam);
            await _context.SaveChangesAsync();

            // Act
            Match? result = await _matchRepository.AddAsync(futureMatch);

            // Assert
            result.Should().NotBeNull();
            homeTeam.Rank.Should().Be(5);
            awayTeam.Rank.Should().Be(3);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenTeamsDoNotExist()
        {
            // Arrange
            Match match = _fixture.Build<Match>()
                .With(m => m.HomeTeamId, Guid.NewGuid())
                .With(m => m.AwayTeamId, Guid.NewGuid())
                .Create();

            await _context.Matches.AddAsync(match);
            await _context.SaveChangesAsync();

            // Act
            bool result = await _matchRepository.DeleteAsync(match.Id);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GiveRank_ShouldIncreaseHomeTeamRank_WhenHomeTeamWins()
        {
            // Arrange
            Team homeTeam = _fixture.Build<Team>().With(t => t.Id, Guid.NewGuid()).With(t => t.Rank, 5).Create();
            Team awayTeam = _fixture.Build<Team>().With(t => t.Id, Guid.NewGuid()).With(t => t.Rank, 3).Create();

            Match completedMatch = _fixture.Build<Match>()
                .With(m => m.HomeTeamId, homeTeam.Id)
                .With(m => m.AwayTeamId, awayTeam.Id)
                .With(m => m.HomeTeamScore, 3)
                .With(m => m.AwayTeamScore, 1)
                .With(m => m.StartTime, DateTime.Now.AddHours(-5))
                .With(m => m.EndTime, DateTime.Now.AddHours(-2))
                .Create();

            await _context.Teams.AddRangeAsync(homeTeam, awayTeam);
            await _context.SaveChangesAsync();

            _teamRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Team>()))
                .Returns((Team team) => new ValueTask<Team>(team));

            // Act
            await _matchRepository.AddAsync(completedMatch);

            await _context.SaveChangesAsync();

            await _matchRepository.UpdateAsync(completedMatch);

            // Assert
            homeTeam.Rank.Should().Be(8); // +3 points for the win
            awayTeam.Rank.Should().Be(3);
        }

        [Fact]
        public async Task GiveRank_ShouldIncreaseAwayTeamRank_WhenAwayTeamWins()
        {
            // Arrange
            Team homeTeam = _fixture.Build<Team>().With(t => t.Id, Guid.NewGuid()).With(t => t.Rank, 5).Create();
            Team awayTeam = _fixture.Build<Team>().With(t => t.Id, Guid.NewGuid()).With(t => t.Rank, 3).Create();

            Match completedMatch = _fixture.Build<Match>()
                .With(m => m.HomeTeamId, homeTeam.Id)
                .With(m => m.AwayTeamId, awayTeam.Id)
                .With(m => m.HomeTeamScore, 1)
                .With(m => m.AwayTeamScore, 3)
                .With(m => m.StartTime, DateTime.Now.AddHours(-5))
                .With(m => m.EndTime, DateTime.Now.AddHours(-2))
                .Create();

            await _context.Teams.AddRangeAsync(homeTeam, awayTeam);
            await _context.SaveChangesAsync();

            _teamRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Team>()))
                .Returns((Team team) => new ValueTask<Team>(team));

            await _matchRepository.AddAsync(completedMatch);

            // Act
            await _matchRepository.UpdateAsync(completedMatch);

            // Assert
            awayTeam.Rank.Should().Be(6); // +3 points for the win
            homeTeam.Rank.Should().Be(5);
        }

        [Fact]
        public async Task GiveRank_ShouldIncreaseBothRanksByOne_WhenMatchIsDraw()
        {
            // Arrange
            Team homeTeam = _fixture.Build<Team>().With(t => t.Id, Guid.NewGuid()).With(t => t.Rank, 5).Create();
            Team awayTeam = _fixture.Build<Team>().With(t => t.Id, Guid.NewGuid()).With(t => t.Rank, 3).Create();

            Match drawMatch = _fixture.Build<Match>()
                .With(m => m.HomeTeamId, homeTeam.Id)
                .With(m => m.AwayTeamId, awayTeam.Id)
                .With(m => m.HomeTeamScore, 2)
                .With(m => m.AwayTeamScore, 2)
                .With(m => m.StartTime, DateTime.Now.AddHours(-5))
                .With(m => m.EndTime, DateTime.Now.AddHours(-2))
                .Create();

            await _context.Teams.AddRangeAsync(homeTeam, awayTeam);
            await _context.SaveChangesAsync();

            _teamRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Team>()))
                .Returns((Team team) => new ValueTask<Team>(team));

            await _matchRepository.AddAsync(drawMatch);

            // Act
            await _matchRepository.UpdateAsync(drawMatch);

            // Assert
            homeTeam.Rank.Should().Be(6); // +1 point
            awayTeam.Rank.Should().Be(4); // +1 point
        }

        [Fact]
        public async Task GiveRank_ShouldNotAdjustRank_WhenMatchIsInFuture()
        {
            // Arrange
            Team homeTeam = _fixture.Build<Team>().With(t => t.Id, Guid.NewGuid()).With(t => t.Rank, 5).Create();
            Team awayTeam = _fixture.Build<Team>().With(t => t.Id, Guid.NewGuid()).With(t => t.Rank, 3).Create();

            Match futureMatch = _fixture.Build<Match>()
                .With(m => m.HomeTeamId, homeTeam.Id)
                .With(m => m.AwayTeamId, awayTeam.Id)
                .With(m => m.HomeTeamScore, 2)
                .With(m => m.AwayTeamScore, 1)
                .With(m => m.StartTime, DateTime.Now.AddHours(1)) // Future match
                .With(m => m.EndTime, DateTime.Now.AddHours(2))
                .Create();

            await _context.Teams.AddRangeAsync(homeTeam, awayTeam);
            await _context.SaveChangesAsync();

            // Act
            await _matchRepository.AddAsync(futureMatch);

            // Assert
            homeTeam.Rank.Should().Be(5); // No change
            awayTeam.Rank.Should().Be(3);
        }

        [Fact]
        public async Task GiveRank_ShouldIncreaseBothRanksByOne_WhenMatchIsDrawAndScoresAreEqual()
        {
            // Arrange
            Team homeTeam = _fixture.Build<Team>()
                .With(t => t.Id, Guid.NewGuid())
                .With(t => t.Rank, 10)
                .Create();

            Team awayTeam = _fixture.Build<Team>()
                .With(t => t.Id, Guid.NewGuid())
                .With(t => t.Rank, 8)
                .Create();

            Match drawMatch = _fixture.Build<Match>()
                .With(m => m.HomeTeamId, homeTeam.Id)
                .With(m => m.AwayTeamId, awayTeam.Id)
                .With(m => m.HomeTeamScore, 2)
                .With(m => m.AwayTeamScore, 2)
                .With(m => m.StartTime, DateTime.Now.AddHours(-5))
                .With(m => m.EndTime, DateTime.Now.AddHours(-2))
                .Create();

            await _context.Teams.AddRangeAsync(homeTeam, awayTeam);
            await _context.SaveChangesAsync();

            _teamRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Team>()))
                .Returns((Team team) => new ValueTask<Team>(team));

            await _matchRepository.AddAsync(drawMatch);
            // Act
            await _matchRepository.UpdateAsync(drawMatch);

            // Assert
            homeTeam.Rank.Should().Be(11); // +1 point
            awayTeam.Rank.Should().Be(9);  // +1 point
        }

        [Fact]
        public async Task GiveRank_ShouldHandleDraw_WhenScoresAreZero()
        {
            // Arrange
            Team homeTeam = _fixture.Build<Team>()
                .With(t => t.Id, Guid.NewGuid())
                .With(t => t.Rank, 5)
                .Create();

            Team awayTeam = _fixture.Build<Team>()
                .With(t => t.Id, Guid.NewGuid())
                .With(t => t.Rank, 7)
                .Create();

            Match zeroScoreDraw = _fixture.Build<Match>()
                .With(m => m.HomeTeamId, homeTeam.Id)
                .With(m => m.AwayTeamId, awayTeam.Id)
                .With(m => m.HomeTeamScore, 0)
                .With(m => m.AwayTeamScore, 0)
                .With(m => m.StartTime, DateTime.Now.AddHours(-3))
                .With(m => m.EndTime, DateTime.Now.AddHours(-1))
                .Create();

            await _context.Teams.AddRangeAsync(homeTeam, awayTeam);
            await _context.SaveChangesAsync();

            _teamRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Team>()))
                .Returns((Team team) => new ValueTask<Team>(team));

            await _matchRepository.AddAsync(zeroScoreDraw);

            // Act
            await _matchRepository.UpdateAsync(zeroScoreDraw);

            // Assert
            homeTeam.Rank.Should().Be(6); // +1 point
            awayTeam.Rank.Should().Be(8); // +1 point
        }

        [Fact]
        public async Task GiveRank_ShouldIncreaseRanks_WhenMatchIsDrawWithHighScores()
        {
            // Arrange
            Team homeTeam = _fixture.Build<Team>()
                .With(t => t.Id, Guid.NewGuid())
                .With(t => t.Rank, 12)
                .Create();

            Team awayTeam = _fixture.Build<Team>()
                .With(t => t.Id, Guid.NewGuid())
                .With(t => t.Rank, 15)
                .Create();

            // Create and add the match to the database
            Match highScoreDraw = _fixture.Build<Match>()
                .With(m => m.Id, Guid.NewGuid())
                .With(m => m.HomeTeamId, homeTeam.Id)
                .With(m => m.AwayTeamId, awayTeam.Id)
                .With(m => m.HomeTeamScore, 5)
                .With(m => m.AwayTeamScore, 5)
                .With(m => m.StartTime, DateTime.Now.AddHours(-10))
                .With(m => m.EndTime, DateTime.Now.AddHours(-7))
                .With(m => m.IsDeleted, false)
                .Create();

            await _context.Teams.AddRangeAsync(homeTeam, awayTeam);
            await _context.SaveChangesAsync();

            _teamRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Team>()))
                .Returns((Team team) => new ValueTask<Team>(team));

            await _matchRepository.AddAsync(highScoreDraw);

            // Act
            Match? result = await _matchRepository.UpdateAsync(highScoreDraw);

            // Assert
            result.Should().NotBeNull();
            homeTeam.Rank.Should().Be(13); // +1 point for draw
            awayTeam.Rank.Should().Be(16); // +1 point for draw
        }

        [Fact]
        public async Task GiveRank_ShouldNotUpdateRanks_WhenMatchIsNotCompleted()
        {
            // Arrange
            Team homeTeam = _fixture.Build<Team>()
                .With(t => t.Id, Guid.NewGuid())
                .With(t => t.Rank, 5)
                .Create();

            Team awayTeam = _fixture.Build<Team>()
                .With(t => t.Id, Guid.NewGuid())
                .With(t => t.Rank, 7)
                .Create();

            Match ongoingMatch = _fixture.Build<Match>()
                .With(m => m.HomeTeamId, homeTeam.Id)
                .With(m => m.AwayTeamId, awayTeam.Id)
                .With(m => m.HomeTeamScore, 2)
                .With(m => m.AwayTeamScore, 2)
                .With(m => m.StartTime, DateTime.Now.AddHours(-1))
                .With(m => m.EndTime, DateTime.Now.AddHours(1)) // Match still ongoing
                .Create();

            await _context.Teams.AddRangeAsync(homeTeam, awayTeam);
            await _context.SaveChangesAsync();

            // Act
            await _matchRepository.AddAsync(ongoingMatch);

            // Assert
            homeTeam.Rank.Should().Be(5);
            awayTeam.Rank.Should().Be(7);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }
    }
}