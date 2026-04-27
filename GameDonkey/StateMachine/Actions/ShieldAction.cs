using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace GameDonkeyLib
{
    public class ShieldAction : TimedAction, IStateActionsList
    {
        #region Properties
        private StateActionsList StateActionsList { get; set; }

        public List<BaseAction> Actions => StateActionsList.Actions;

        #endregion //Properties

        #region Initialization

        public ShieldAction(BaseObject owner, EActionType actionType = EActionType.Shield) :
            base(owner, actionType)
        {
            StateActionsList = new StateActionsList();
        }

        public ShieldAction(BaseObject owner, ShieldActionModel actionModel, IStateContainer stateContainer) :
            base(owner, actionModel, actionModel.TimeDelta)
        {
            StateActionsList = new StateActionsList();
            StateActionsList.LoadStateActions(actionModel.ActionModels, owner, stateContainer);
        }

        public ShieldAction(BaseObject owner, BaseActionModel actionModel, IStateContainer stateContainer) :
            this(owner, actionModel as ShieldActionModel, stateContainer)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            StateActionsList.LoadContent(engine, content);
        }

        #endregion //Initialization

        #region Methods
        public override bool Execute(float currentTime)
        {
            AddBlock();

            //reset teh success actions
            for (int i = 0; i < StateActionsList.Actions.Count; i++)
            {
                StateActionsList.Actions[i].Reset();
            }

            return base.Execute(currentTime);
        }

        protected virtual void AddBlock()
        {
            //add this action to the list of block states
            Owner.ShieldActions.AddAction(this, Owner.CharacterClock);
        }
        public bool ExecuteSuccessActions()
        {
            var result = false;
            for (int i = 0; i < StateActionsList.Actions.Count; i++)
            {
                //TODO: does this require the actual time?
                if (StateActionsList.Actions[i].Execute(0f))
                {
                    result = true;
                }
            }

            return result;
        }

        public BaseAction AddNewActionFromType(EActionType actionType, BaseObject owner, IGameDonkey engine, ContentManager content)
        {
            return StateActionsList.AddNewActionFromType(actionType, owner, engine, content);
        }

        public void LoadStateActions(StateActionsListModel actionModels, BaseObject owner, IStateContainer stateContainer)
        {
            StateActionsList.LoadStateActions(actionModels, owner, stateContainer);
        }

        public bool RemoveAction(BaseAction action)
        {
            return StateActionsList.RemoveAction(action);
        }

        public void Sort()
        {
            StateActionsList.Sort();
        }

        #endregion //Methods
    }
}