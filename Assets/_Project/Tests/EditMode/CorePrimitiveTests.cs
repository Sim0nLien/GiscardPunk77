using NUnit.Framework;

namespace GiscardPunk77.Core.Tests
{
    public sealed class CorePrimitiveTests
    {
        [Test]
        public void ActorKind_UsesStableDefaultValue()
        {
            Assert.That((int)ActorKind.Unknown, Is.EqualTo(0));
        }

        [Test]
        public void TeamId_ComparesByValue()
        {
            Assert.That(new TeamId(7), Is.EqualTo(new TeamId(7)));
            Assert.That(new TeamId(7), Is.Not.EqualTo(new TeamId(8)));
            Assert.That(default(TeamId), Is.EqualTo(TeamId.Neutral));
        }

        [Test]
        public void ActorIdentity_ContainsKindAndTeam()
        {
            TeamId team = new TeamId(2);
            ActorIdentity identity = new ActorIdentity(ActorKind.Guard, team);

            Assert.That(identity.Kind, Is.EqualTo(ActorKind.Guard));
            Assert.That(identity.Team, Is.EqualTo(team));
            Assert.That(
                identity,
                Is.EqualTo(new ActorIdentity(ActorKind.Guard, new TeamId(2))));
        }
    }
}
