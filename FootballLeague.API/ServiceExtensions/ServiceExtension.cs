using FootballLeague.Data;
using FootballLeague.Data.Interfaces;
using FootballLeague.Data.Repositories;
using FootballLeague.Domain.Interfaces;
using FootballLeague.Domain.Services;

namespace FootballLeague.API.ServiceExtensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            // DATA SEEDER
            services.AddScoped<DataSeeder>();

            // SERVICES
            services.AddTransient<ITeamService, TeamService>();
            services.AddTransient<IMatchService, MatchService>();

            // REPOS
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<IMatchRepository, MatchRepository>();

            return services;
        }
    }
}