using GameDonkeyLib;
using GameTimer;

namespace GameDonkey.Tests
{
    public class TestObject : BaseObject
    {
        public bool KillPlayerCalled { get; private set; }

        public TestObject() : base(GameObjectType.Human, new HitPauseClock(), 0, "Test")
        {
        }

        protected override void Init()
        {
            // Skip Physics/States creation for unit tests
        }

        public override void KillPlayer()
        {
            KillPlayerCalled = true;
        }
    }
}
