using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    // Tests for TimedAction abstract class behavior, exercised via TrailAction
    [TestFixture]
    public class TimedActionTests
    {
        [Test]
        public void TimeDelta_SetToMinusOne_SetsActiveForWholeStateTrue()
        {
            var action = new TrailAction(null);

            action.TimeDelta = -1f;

            action.ActiveForWholeState.ShouldBeTrue();
        }

        [Test]
        public void TimeDelta_SetToPositiveValue_SetsActiveForWholeStateFalse()
        {
            var action = new TrailAction(null);

            action.TimeDelta = 2f;

            action.ActiveForWholeState.ShouldBeFalse();
        }

        [Test]
        public void TimeDelta_DefaultValue_IsMinusOne()
        {
            var action = new TrailAction(null);

            action.TimeDelta.ShouldBe(-1f);
            action.ActiveForWholeState.ShouldBeTrue();
        }

        [Test]
        public void SetDoneTime_SetsBasedOnClockCurrentTimePlusTimeDelta()
        {
            var owner = new TestObject();
            var action = new TrailAction(owner) { TimeDelta = 3f };

            action.SetDoneTime(owner.CharacterClock);

            action.DoneTime.ShouldBe(3f);
        }
    }
}
