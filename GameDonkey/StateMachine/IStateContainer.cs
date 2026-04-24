using GameTimer;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;
using System;
using System.Collections.Generic;

namespace GameDonkeyLib
{
    public interface IStateContainer
    {
        #region Events
        event EventHandler<StateChangeEventArgs<string>> StateChangedEvent;

        #endregion //Events

        #region Properties

        StateMachineActions Actions { get; }
        string Name { get; }
        StringStateMachine StateMachine { get; }

        string CurrentState { get; }

        string PrevState { get; }

        GameClock StateClock { get; }

        #endregion //Properties

        #region Methods

        void Reset();
        bool SendStateMessage(string message);

        void ForceStateChange(string state);
        void StateChange(object sender, StateChangeEventArgs<string> e);
        void ExecuteActions(GameClock clock);
        bool IsCurrentStateAttack();
        bool IsStateAttack(string state);
        bool IsAttackMessage(string message);
        bool IsAttackActive();
        void ReplaceOwner(BaseObject bot);

        SingleStateActions GetStateActions(string stateName);

        void LoadContent(BaseObjectModel baseObjectmodel, BaseObject owner, IGameDonkey engine, ContentManager content);

        void WriteXml();

        #endregion //Methods
    }
}