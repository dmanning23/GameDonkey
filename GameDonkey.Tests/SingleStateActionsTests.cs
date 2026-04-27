using GameDonkeyLib;
using GameTimer;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class SingleStateActionsTests
    {
        [Test]
        public void StateChange_ResetsAllChildActions()
        {
            var owner = new TestObject();
            var state = new SingleStateActions();
            var a1 = new RotateAction(owner) { Time = 0f };
            var a2 = new RotateAction(owner) { Time = 1f };
            state.Actions.Add(a1);
            state.Actions.Add(a2);
            // Both have AlreadyRun=true by default

            state.StateChange();

            a1.AlreadyRun.ShouldBeFalse();
            a2.AlreadyRun.ShouldBeFalse();
        }

        [Test]
        public void ReplaceOwner_UpdatesOwnerOnAllActions()
        {
            var owner1 = new TestObject();
            var owner2 = new TestObject();
            var state = new SingleStateActions();
            var action = new RotateAction(owner1);
            state.Actions.Add(action);

            state.ReplaceOwner(owner2);

            action.Owner.ShouldBeSameAs(owner2);
        }

        [Test]
        public void IsAttackActive_ReturnsFalse_WhenNoPriorLoadContent()
        {
            var state = new SingleStateActions();
            var clock = new GameClock();

            var result = state.IsAttackActive(clock);

            result.ShouldBeFalse();
        }

        [Test]
        public void IsAttack_DefaultsFalse()
        {
            var state = new SingleStateActions();

            state.IsAttack.ShouldBeFalse();
        }

        [Test]
        public void ToString_ReturnsStateName()
        {
            var state = new SingleStateActions() { StateName = "idle" };

            state.ToString().ShouldBe("idle");
        }
    }
}
