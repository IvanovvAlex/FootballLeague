using AutoFixture;
using FluentAssertions;
using FootballLeague.Data;
using FootballLeague.Data.Entities;
using FootballLeague.Data.Repositories;
using FootballLeague.Tests.Utils;
using Microsoft.EntityFrameworkCore;

namespace FootballLeague.Tests.Unit.Repositories
{
    public class TeamRepositoryTests : IDisposable
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly AppDbContext _context;
        private readonly TeamRepository _teamRepository;

        public TeamRepositoryTests()
        {
            _fixture.Customize(new ActiveCustomization());
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
           .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            DbContextOptions<AppDbContext>? options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"TeamsTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new(options);
            _teamRepository = new(_context);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnNonDeletedTeams()
        {
            // Arrange
            List<Team> teams = _fixture.Build<Team>()
                .With(t => t.IsDeleted, false)
                .CreateMany(5)
                .ToList();

            List<Team> deletedTeams = _fixture.Build<Team>()
                .With(t => t.IsDeleted, true)
                .CreateMany(2)
                .ToList();

            await _context.Teams.AddRangeAsync(teams.Concat(deletedTeams));
            await _context.SaveChangesAsync();

            // Act
            IEnumerable<Team> result = await _teamRepository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
            result.Should().OnlyContain(t => t.IsDeleted == false);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectTeam()
        {
            // Arrange
            Team team = _fixture.Build<Team>()
                .With(t => t.IsDeleted, false)
                .Create();
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            // Act
            Team? result = await _teamRepository.GetByIdAsync(team.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Team>();
            result.Should().BeEquivalentTo(team);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNullForDeletedTeam()
        {
            // Arrange
            Team team = _fixture.Build<Team>()
                .With(t => t.IsDeleted, true)
                .Create();
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            // Act
            Team? result = await _teamRepository.GetByIdAsync(team.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task IsTeamExists_ShouldReturnTrueForExistingTeam()
        {
            // Arrange
            Team team = _fixture.Build<Team>()
                .With(t => t.IsDeleted, false)
                .Create();
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            // Act
            bool result = await _teamRepository.IsTeamExists(team.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsTeamExists_ShouldReturnFalseForNonExistingTeam()
        {
            // Arrange
            Guid nonExistingTeamId = Guid.NewGuid();

            // Act
            bool result = await _teamRepository.IsTeamExists(nonExistingTeamId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsTeamNameAlreadyInUse_ShouldReturnTrueIfNameExists()
        {
            // Arrange
            Team team = _fixture.Build<Team>()
                .With(t => t.Name, "ExistingTeam")
                .With(t => t.IsDeleted, false)
                .Create();
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            // Act
            bool result = await _teamRepository.IsTeamNameAlreadyInUse("ExistingTeam");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsTeamNameAlreadyInUse_ShouldReturnFalseIfNameNotExists()
        {
            // Act
            bool result = await _teamRepository.IsTeamNameAlreadyInUse("NonExistingTeam");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoTeamsExist()
        {
            // Arrange
            // Database is already empty

            // Act
            IEnumerable<Team> result = await _teamRepository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenTeamDoesNotExist()
        {
            // Arrange
            Guid nonExistingTeamId = Guid.NewGuid();

            // Act
            Team? result = await _teamRepository.GetByIdAsync(nonExistingTeamId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task IsTeamExists_ShouldReturnFalse_WhenTeamIsDeleted()
        {
            // Arrange
            Team team = _fixture.Build<Team>()
                .With(t => t.IsDeleted, true)
                .Create();
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            // Act
            bool result = await _teamRepository.IsTeamExists(team.Id);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsTeamNameAlreadyInUse_ShouldReturnFalse_WhenTeamIsDeleted()
        {
            // Arrange
            Team team = _fixture.Build<Team>()
                .With(t => t.Name, "DeletedTeam")
                .With(t => t.IsDeleted, true)
                .Create();

            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            // Act
            bool result = await _teamRepository.IsTeamNameAlreadyInUse("DeletedTeam");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldNotIncludeDeletedMatches()
        {
            // Arrange
            Match deletedMatch = _fixture.Build<Match>()
                .With(m => m.IsDeleted, true)
                .Create();

            Match activeMatch = _fixture.Build<Match>()
                .With(m => m.IsDeleted, false)
                .Create();

            Team team = _fixture.Build<Team>()
                .With(t => t.IsDeleted, false)
                .With(t => t.HomeMatches, new List<Match> { deletedMatch, activeMatch })
                .Create();

            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            // Act
            Team? result = await _teamRepository.GetByIdAsync(team.Id);

            // Assert
            result.Should().NotBeNull();
            result!.HomeMatches.Should().HaveCount(2);
            result.HomeMatches.Should().Contain(m => m.IsDeleted == true);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnTeamsInDescendingRankOrder()
        {
            // Arrange
            List<Team> teams = _fixture.Build<Team>()
                .With(t => t.IsDeleted, false)
                .CreateMany(5)
                .OrderBy(t => t.Rank) // Randomize rank
                .ToList();

            await _context.Teams.AddRangeAsync(teams);
            await _context.SaveChangesAsync();

            // Act
            IEnumerable<Team> result = await _teamRepository.GetAllAsync();

            // Assert
            result.Should().BeInDescendingOrder(t => t.Rank);
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