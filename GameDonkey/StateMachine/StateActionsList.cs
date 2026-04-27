using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Linq;

namespace GameDonkeyLib
{
    public class StateActionsList : IStateActionsList
    {
        #region Properties
        public List<BaseAction> Actions { get; private set; }

        #endregion //Properties

        #region Methods

        #region Initialization
        public StateActionsList()
        {
            Actions = new List<BaseAction>();
        }

        public virtual void LoadStateActions(StateActionsListModel actionModels, BaseObject owner, IStateContainer stateContainer)
        {
            for (int i = 0; i < actionModels.ActionModels.Count; i++)
            {
                var stateAction = StateActionFactory.CreateStateAction(actionModels.ActionModels[i], owner, stateContainer);
                Actions.Add(stateAction);
            }
        }

        public virtual void LoadContent(IGameDonkey engine, ContentManager content)
        {
            for (int i = 0; i < Actions.Count; i++)
            {
                Actions[i].LoadContent(engine, content);
            }

            Sort();
        }

        #endregion //Initialization

        #region Tool Methods
        public BaseAction AddNewActionFromType(EActionType actionType, BaseObject owner, IGameDonkey engine, ContentManager content)
        {
            //get the correct action type
            var action = StateActionFactory.CreateStateAction(actionType, owner);
            action.LoadContent(engine, content);

            //save the action
            Actions.Add(action);

            //sort the list of actions
            Sort();

            //return the newly created dude
            return action;
        }
        public bool RemoveAction(BaseAction action)
        {
            return Actions.Remove(action);
        }

        public void Sort()
        {
            Actions.Sort(new ActionSort());
        }

        public BaseAction FindAction(string id)
        {
            return Actions.FirstOrDefault(x => x.Id == id);
        }

        #endregion //Tool Methods

        public bool ExecuteAction(float currentTime)
        {
            //loop through all actions, execute the ones between the time slice
            for (int i = 0; i < Actions.Count; i++)
            {
                //first check if the time of this action is expired
                if (Actions[i].Time > currentTime)
                {
                    //this action doesnt need to be run yet!
                    return false;
                }

                //check if this action hasn't happened yet
                else if (!Actions[i].AlreadyRun)
                {
                    if (Actions[i].Execute(currentTime))
                    {
                        //the state was changed when that dude was running
                        return true;
                    }
                }
            }

            //The state was not changed while these actions were running.
            return false;
        }

        #endregion //Methods
    }
}