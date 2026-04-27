using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class StateActionsListTests
    {
        [Test]
        public void ExecuteAction_RunsAction_WhenTimeMatches()
        {
            var owner = new TestObject();
            var list = new StateActionsList();
            var action = new RotateAction(owner) { Time = 0f, Rotation = 3f };
            list.Actions.Add(action);
            action.Reset();

            list.ExecuteAction(1f);

            owner.RotationPerSecond.ShouldBe(3f);
        }

        [Test]
        public void ExecuteAction_SkipsAction_WhenTimeNotReached()
        {
            var owner = new TestObject();
            var list = new StateActionsList();
            var action = new RotateAction(owner) { Time = 5f, Rotation = 3f };
            list.Actions.Add(action);
            action.Reset();

            list.ExecuteAction(1f);

            owner.RotationPerSecond.ShouldBe(0f);
        }

        [Test]
        public void ExecuteAction_SkipsAlreadyRunAction()
        {
            var owner = new TestObject();
            var list = new StateActionsList();
            var action = new RotateAction(owner) { Time = 0f, Rotation = 3f };
            list.Actions.Add(action);
            // AlreadyRun=true by default — no Reset called

            list.ExecuteAction(1f);

            owner.RotationPerSecond.ShouldBe(0f);
        }

        [Test]
        public void ExecuteAction_ReturnsTrue_WhenActionTriggersStateChange()
        {
            var owner = new TestObject();
            var stub = new StubStateContainer();
            var list = new StateActionsList();
            var messageModel = new SendStateMessageActionModel() { Message = "hit", Time = 0f };
            var action = new SendStateMessageAction(owner, messageModel, stub);
            list.Actions.Add(action);
            action.Reset();

            var result = list.ExecuteAction(1f);

            result.ShouldBeTrue();
        }

        [Test]
        public void ExecuteAction_ReturnsFalse_WhenNoStateChange()
        {
            var owner = new TestObject();
            var list = new StateActionsList();
            var action = new RotateAction(owner) { Time = 0f };
            list.Actions.Add(action);
            action.Reset();

            var result = list.ExecuteAction(1f);

            result.ShouldBeFalse();
        }

        [Test]
        public void Sort_OrdersActionsByTime()
        {
            var list = new StateActionsList();
            var a1 = new RotateAction(null) { Time = 3f };
            var a2 = new RotateAction(null) { Time = 1f };
            var a3 = new RotateAction(null) { Time = 2f };
            list.Actions.Add(a1);
            list.Actions.Add(a2);
            list.Actions.Add(a3);

            list.Sort();

            list.Actions[0].Time.ShouldBe(1f);
            list.Actions[1].Time.ShouldBe(2f);
            list.Actions[2].Time.ShouldBe(3f);
        }

        [Test]
        public void Sort_SameTime_OrdersByActionType()
        {
            var list = new StateActionsList();
            var rotate = new RotateAction(null) { Time = 1f };
            var setVel = new SetVelocityAction(null) { Time = 1f };
            list.Actions.Add(rotate);
            list.Actions.Add(setVel);

            list.Sort();

            // EActionType enum values determine order
            ((int)list.Actions[0].ActionType).ShouldBeLessThan((int)list.Actions[1].ActionType);
        }

        [Test]
        public void RemoveAction_RemovesFromList()
        {
            var list = new StateActionsList();
            var action = new RotateAction(null);
            list.Actions.Add(action);

            var removed = list.RemoveAction(action);

            removed.ShouldBeTrue();
            list.Actions.Count.ShouldBe(0);
        }

        [Test]
        public void RemoveAction_ReturnsFalse_WhenNotPresent()
        {
            var list = new StateActionsList();
            var action = new RotateAction(null);

            var removed = list.RemoveAction(action);

            removed.ShouldBeFalse();
        }

        [Test]
        public void FindAction_ReturnsActionById()
        {
            var list = new StateActionsList();
            var action = new RotateAction(null) { Id = "spin" };
            list.Actions.Add(action);

            var found = list.FindAction("spin");

            found.ShouldBeSameAs(action);
        }

        [Test]
        public void FindAction_ReturnsNull_WhenNotFound()
        {
            var list = new StateActionsList();

            var found = list.FindAction("missing");

            found.ShouldBeNull();
        }
    }
}
