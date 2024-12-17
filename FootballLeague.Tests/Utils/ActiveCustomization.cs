using AutoFixture;
using FootballLeague.Data.Entities;

namespace FootballLeague.Tests.Utils
{
    public class ActiveCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<Team>(composer => composer.With(x => x.IsDeleted, false));
            fixture.Customize<Match>(composer => composer.With(x => x.IsDeleted, false));
        }
    }
}