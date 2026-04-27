using GameDonkeyLib;
using GameTimer;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class TimedActionListTests
    {
        [Test]
        public void AddAction_AddsToList()
        {
            var owner = new TestObject();
            var list = new TimedActionList<TrailAction>();
            var action = new TrailAction(owner) { TimeDelta = 2f };

            list.AddAction(action, owner.CharacterClock);

            list.CurrentActions.Count.ShouldBe(1);
            list.CurrentActions[0].ShouldBeSameAs(action);
        }

        [Test]
        public void AddAction_SetsDoneTime()
        {
            var owner = new TestObject();
            var list = new TimedActionList<TrailAction>();
            var action = new TrailAction(owner) { TimeDelta = 3f };

            list.AddAction(action, owner.CharacterClock);

            // CharacterClock.CurrentTime=0 + TimeDelta=3 = 3
            action.DoneTime.ShouldBe(3f);
        }

        [Test]
        public void Reset_ClearsList()
        {
            var owner = new TestObject();
            var list = new TimedActionList<TrailAction>();
            list.AddAction(new TrailAction(owner) { TimeDelta = 2f }, owner.CharacterClock);

            list.Reset();

            list.CurrentActions.Count.ShouldBe(0);
        }

        [Test]
        public void Update_RemovesExpiredAction()
        {
            var owner = new TestObject();
            var list = new TimedActionList<TrailAction>();
            var action = new TrailAction(owner) { TimeDelta = 0f }; // DoneTime=0 when added at t=0
            list.AddAction(action, owner.CharacterClock);

            list.Update(owner.CharacterClock); // CurrentTime=0, DoneTime=0 → 0<=0 → remove

            list.CurrentActions.Count.ShouldBe(0);
        }

        [Test]
        public void Update_KeepsNonExpiredAction()
        {
            var owner = new TestObject();
            var list = new TimedActionList<TrailAction>();
            var action = new TrailAction(owner) { TimeDelta = 5f }; // DoneTime=5
            list.AddAction(action, owner.CharacterClock);

            list.Update(owner.CharacterClock); // CurrentTime=0, DoneTime=5 → 5>0 → keep

            list.CurrentActions.Count.ShouldBe(1);
        }

        [Test]
        public void Update_KeepsActiveForWholeStateActions()
        {
            var owner = new TestObject();
            var list = new TimedActionList<TrailAction>();
            var action = new TrailAction(owner); // TimeDelta=-1 → ActiveForWholeState=true
            list.AddAction(action, owner.CharacterClock);

            list.Update(owner.CharacterClock);

            list.CurrentActions.Count.ShouldBe(1);
        }
    }
}
