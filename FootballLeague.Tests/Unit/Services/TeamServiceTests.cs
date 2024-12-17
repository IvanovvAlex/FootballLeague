using AutoFixture;
using FluentAssertions;
using FootballLeague.Common.Requests.Team;
using FootballLeague.Common.Responses.Team;
using FootballLeague.Data.Entities;
using FootballLeague.Data.Interfaces;
using FootballLeague.Domain.Services;
using Moq;

namespace FootballLeague.Tests.Unit.Services
{
    public class TeamServiceTests
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly Mock<ITeamRepository> _teamRepositoryMock;
        private readonly TeamService _teamService;

        public TeamServiceTests()
        {
            _teamRepositoryMock = new Mock<ITeamRepository>();
            _teamService = new TeamService(_teamRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateTeam_WhenNameIsUnique()
        {
            // Arrange
            CreateTeamRequest request = _fixture.Build<CreateTeamRequest>()
                .With(r => r.Name, "Unique Team Name")
                .Create();

            _teamRepositoryMock.Setup(repo => repo.IsTeamNameAlreadyInUse(request.Name))
                .ReturnsAsync(false);

            _teamRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Team>()))
    .Returns((Team team) => new ValueTask<Team>(team));

            // Act
            TeamResponse result = await _teamService.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(request.Name);
            result.Id.Should().NotBeEmpty();

            _teamRepositoryMock.Verify(repo => repo.IsTeamNameAlreadyInUse(request.Name), Times.Once);
            _teamRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Team>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenNameAlreadyExists()
        {
            // Arrange
            CreateTeamRequest request = _fixture.Create<CreateTeamRequest>();

            _teamRepositoryMock.Setup(repo => repo.IsTeamNameAlreadyInUse(request.Name))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _teamService.CreateAsync(request);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Team name already exists!");

            _teamRepositoryMock.Verify(repo => repo.IsTeamNameAlreadyInUse(request.Name), Times.Once);
            _teamRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Team>()), Times.Never);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnEmptyList_WhenNoTeamsExist()
        {
            // Arrange
            _teamRepositoryMock.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<Team>());

            // Act
            IEnumerable<TeamResponse> result = await _teamService.GetAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _teamRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnUpdatedTeamResponse_WhenUpdateIsSuccessful()
        {
            // Arrange
            UpdateTeamRequest request = _fixture.Create<UpdateTeamRequest>();
            Team updatedTeam = new Team
            {
                Id = request.Id,
                Name = request.Name,
                Rank = 10
            };

            _teamRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Team>()))
    .Returns((Team team) => new ValueTask<Team>(updatedTeam));

            // Act
            TeamResponse? result = await _teamService.UpdateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(request.Id);
            result.Name.Should().Be(request.Name);
            result.Rank.Should().Be(10);

            _teamRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Team>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenTeamDoesNotExist()
        {
            // Arrange
            UpdateTeamRequest request = _fixture.Create<UpdateTeamRequest>();

            _teamRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Team>()))
      .Returns((Team team) => new ValueTask<Team>());

            // Act
            TeamResponse? result = await _teamService.UpdateAsync(request);

            // Assert
            result.Should().BeNull();

            _teamRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Team>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenTeamIsDeleted()
        {
            // Arrange
            Guid teamId = Guid.NewGuid();

            _teamRepositoryMock.Setup(repo => repo.IsTeamExists(teamId))
                .ReturnsAsync(true);

            _teamRepositoryMock.Setup(repo => repo.DeleteAsync(teamId))
                .ReturnsAsync(true);

            // Act
            bool result = await _teamService.DeleteAsync(teamId);

            // Assert
            result.Should().BeTrue();

            _teamRepositoryMock.Verify(repo => repo.IsTeamExists(teamId), Times.Once);
            _teamRepositoryMock.Verify(repo => repo.DeleteAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenTeamDoesNotExist()
        {
            // Arrange
            Guid teamId = Guid.NewGuid();

            _teamRepositoryMock.Setup(repo => repo.IsTeamExists(teamId))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _teamService.DeleteAsync(teamId);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Team does not exist!");

            _teamRepositoryMock.Verify(repo => repo.IsTeamExists(teamId), Times.Once);
            _teamRepositoryMock.Verify(repo => repo.DeleteAsync(teamId), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenDeleteFails()
        {
            // Arrange
            Guid teamId = Guid.NewGuid();

            _teamRepositoryMock.Setup(repo => repo.IsTeamExists(teamId))
                .ReturnsAsync(true);

            _teamRepositoryMock.Setup(repo => repo.DeleteAsync(teamId))
                .ReturnsAsync(false);

            // Act
            bool result = await _teamService.DeleteAsync(teamId);

            // Assert
            result.Should().BeFalse();

            _teamRepositoryMock.Verify(repo => repo.IsTeamExists(teamId), Times.Once);
            _teamRepositoryMock.Verify(repo => repo.DeleteAsync(teamId), Times.Once);
        }
    }
}