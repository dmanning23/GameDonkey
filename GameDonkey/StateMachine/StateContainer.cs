using FilenameBuddy;
using GameTimer;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameDonkeyLib
{
    // doesn't subscribe to all state change messages — use singlestatecontainer standalone
    public class StateContainer : IStateContainer
    {
        #region Properties

        private Filename StateContainerFilename;

        private Filename StateMachineFilename;

        public event EventHandler<StateChangeEventArgs<string>> StateChangedEvent;

        public StringStateMachine StateMachine { get; private set; }

        public StateMachineActions Actions { get; private set; }

        public string Name { get; set; }

        public string CurrentState
        {
            get
            {
                return StateMachine.CurrentState;
            }
        }

        public string PrevState
        {
            get
            {
                return StateMachine.PrevState;
            }
        }

        public GameClock StateClock { get; protected set; }

        #endregion //Properties

        #region Initialization

        public StateContainer(string containerName = "") : this(new StringStateMachine(), containerName)
        {
        }

        public StateContainer(StringStateMachine stateMachine, string containerName = "")
        {
            StateMachine = stateMachine;
            Actions = new StateMachineActions();
            StateClock = new GameClock();

            StateMachine.ResetEvent += this.StateChange;
            stateMachine.StateChangedEvent += this.StateChange;
            Name = containerName;

            StateContainerFilename = new Filename();
            StateMachineFilename = new Filename();
        }

        public void LoadContent(BaseObjectModel baseObjectmodel, BaseObject owner, IGameDonkey engine, ContentManager content)
        {
            LoadContent(baseObjectmodel.States, owner, engine, content);
        }

        public void LoadContent(StatesModel stateContainerModel, BaseObject owner, IGameDonkey engine, ContentManager content)
        {
            StateContainerFilename = new Filename(stateContainerModel.StateContainerFilename);
            StateMachineFilename = new Filename(stateContainerModel.StateMachineFilename);

            LoadStateMachine(StateMachine, StateMachineFilename, content);

            using (var StateContainerModel = new StateContainerModel(StateContainerFilename))
            {
                StateContainerModel.ReadXmlFile(content);

                LoadContainer(StateContainerModel, owner);
            }

            Actions.LoadContent(engine, content);
        }

        protected virtual void LoadContainer(StateContainerModel stateContainerModel, BaseObject owner)
        {
            Actions.LoadStateActions(StateMachine.States, stateContainerModel, owner, this);
        }

        public virtual void LoadStateMachine(StringStateMachine machine, Filename file, ContentManager content)
        {
            if (file.HasFilename)
            {
                machine.LoadXml(file, content);
            }
        }

        public void WriteXml()
        {
            using (var model = new StateContainerModel(StateContainerFilename, this))
            {
                model.WriteXml();
            }

            if (StateMachineFilename.HasFilename)
            {
                using (var model = new StateMachineModel(StateMachineFilename, StateMachine))
                {
                    model.WriteXml();
                }
            }
        }

        #endregion //Initialization

        #region Methods

        public void Reset()
        {
            StateMachine.ResetToInitialState();
        }

        public bool SendStateMessage(string message)
        {
            return StateMachine.SendStateMessage(message);
        }

        public void ForceStateChange(string state)
        {
            StateMachine.ForceState(state);
        }

        public void StateChange(object sender, StateChangeEventArgs<string> eventArgs)
        {
            Actions.StateChange(eventArgs.NewState);

            StateClock.Start();
            StateClock.TimeDelta = 0.0f;

            StateChangedEvent?.Invoke(this, eventArgs);
        }

        public void ExecuteActions(GameClock gameClock)
        {
            StateClock.Update(gameClock);

            Actions.ExecuteActions(StateClock, StateMachine.CurrentState);
        }

        public bool IsCurrentStateAttack()
        {
            return IsStateAttack(StateMachine.CurrentState);
        }

        public bool IsStateAttack(string state)
        {
            return Actions.IsStateAttack(state);
        }

        // used to queue moves during combo: true means still in attack phase, not recovery
        public bool IsAttackActive()
        {
            return Actions.IsAttackActive(StateClock, StateMachine.CurrentState);
        }

        public void ReplaceOwner(BaseObject bot)
        {
            Actions.ReplaceOwner(bot);
        }

        public override string ToString()
        {
            return Name;
        }

        public SingleStateActions GetStateActions(string stateName)
        {
            return Actions.GetStateActions(stateName);
        }

        public bool IsAttackMessage(string message)
        {
            return false;
        }

        #endregion //Methods
    }
}