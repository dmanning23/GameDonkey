using GameDonkeyLib;
using GameTimer;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;

namespace GameDonkey.Tests
{
    public class StubStateContainer : IStateContainer
    {
        public List<string> ReceivedMessages { get; } = new();

        public event EventHandler<StateChangeEventArgs<string>>? StateChangedEvent { add { } remove { } }

        public StateMachineActions Actions => null!;
        public string Name => "StubState";
        public StringStateMachine StateMachine => null!;
        public string CurrentState => string.Empty;
        public string PrevState => string.Empty;
        public GameClock StateClock => null!;

        public bool SendStateMessage(string message)
        {
            ReceivedMessages.Add(message);
            return true;
        }

        public void Reset() { }
        public void ForceStateChange(string state) { }
        public void StateChange(object sender, StateChangeEventArgs<string> e) { }
        public void ExecuteActions(GameClock clock) { }
        public bool IsCurrentStateAttack() => false;
        public bool IsStateAttack(string state) => false;
        public bool IsAttackMessage(string message) => false;
        public bool IsAttackActive() => false;
        public void ReplaceOwner(BaseObject bot) { }
        public SingleStateActions GetStateActions(string stateName) => null!;
        public void LoadContent(BaseObjectModel baseObjectmodel, BaseObject owner, IGameDonkey engine, ContentManager content) { }
        public void WriteXml() { }
    }
}
