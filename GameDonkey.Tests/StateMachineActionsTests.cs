using GameDonkeyLib;
using NUnit.Framework;
using Shouldly;

namespace GameDonkey.Tests
{
    [TestFixture]
    public class StateMachineActionsTests
    {
        private StateMachineActions BuildWithState(string stateName, params BaseAction[] actions)
        {
            var machine = new StateMachineActions();
            var state = new SingleStateActions() { StateName = stateName };
            foreach (var a in actions)
                state.Actions.Add(a);
            machine.Actions[stateName] = state;
            return machine;
        }

        [Test]
        public void AddStateMachineActions_MergesNewStates()
        {
            var machine = BuildWithState("idle");
            var other = BuildWithState("attack");

            machine.AddStateMachineActions(other);

            machine.Actions.ContainsKey("idle").ShouldBeTrue();
            machine.Actions.ContainsKey("attack").ShouldBeTrue();
        }

        [Test]
        public void AddStateMachineActions_DoesNotOverwriteExistingState()
        {
            var owner = new TestObject();
            var original = new RotateAction(owner);
            var machine = BuildWithState("idle", original);

            var other = BuildWithState("idle", new RotateAction(owner));
            machine.AddStateMachineActions(other);

            machine.Actions["idle"].Actions[0].ShouldBeSameAs(original);
        }

        [Test]
        public void RemoveStateMachineActions_RemovesMatchingStates()
        {
            var machine = BuildWithState("idle");
            machine.Actions["attack"] = new SingleStateActions() { StateName = "attack" };

            var toRemove = new StateMachineActions();
            toRemove.Actions["attack"] = new SingleStateActions();
            machine.RemoveStateMachineActions(toRemove);

            machine.Actions.ContainsKey("idle").ShouldBeTrue();
            machine.Actions.ContainsKey("attack").ShouldBeFalse();
        }

        [Test]
        public void StateChange_ResetsChildActionsInNamedState()
        {
            var owner = new TestObject();
            var action = new RotateAction(owner);
            var machine = BuildWithState("idle", action);
            // action.AlreadyRun=true by default

            machine.StateChange("idle");

            action.AlreadyRun.ShouldBeFalse();
        }

        [Test]
        public void StateChange_EmptyString_DoesNotThrow()
        {
            var machine = new StateMachineActions();

            Should.NotThrow(() => machine.StateChange(string.Empty));
        }

        [Test]
        public void IsStateAttack_ReturnsFalse_ForNonAttackState()
        {
            var machine = BuildWithState("idle");

            machine.IsStateAttack("idle").ShouldBeFalse();
        }

        [Test]
        public void GetStateActions_ReturnsCorrectEntry()
        {
            var owner = new TestObject();
            var action = new RotateAction(owner);
            var machine = BuildWithState("idle", action);

            var result = machine.GetStateActions("idle");

            result.ShouldNotBeNull();
            result.Actions[0].ShouldBeSameAs(action);
        }

        [Test]
        public void ReplaceOwner_UpdatesOwnerInAllStates()
        {
            var owner1 = new TestObject();
            var owner2 = new TestObject();
            var action = new RotateAction(owner1);
            var machine = BuildWithState("idle", action);

            machine.ReplaceOwner(owner2);

            action.Owner.ShouldBeSameAs(owner2);
        }
    }
}
