using AnimationLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using FilenameBuddy;
using System;
using System.Linq;

namespace GameDonkeyLib
{
    public class TemplateAction : BaseAction, IStateActionsList
    {
        #region Properties

        public StateActionsList StateActionsList { get; private set; }

        public List<BaseAction> Actions => StateActionsList.Actions;

        public Filename FileName { get; set; }

        private IGameDonkey Engine { get; set; }

        #endregion //Properties

        #region Initialization

        public TemplateAction(BaseObject owner, EActionType actionType = EActionType.TemplateAction) :
            base(owner, actionType)
        {
            StateActionsList = new StateActionsList();
        }

        public TemplateAction(BaseObject owner, TemplateActionModel actionModel, IStateContainer stateContainer) :
            base(owner, actionModel)
        {
            StateActionsList = new StateActionsList();
            FileName = new Filename(actionModel.Filename);
        }

        public TemplateAction(BaseObject owner, BaseActionModel actionModel, IStateContainer stateContainer) :
            this(owner, actionModel as TemplateActionModel, stateContainer)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            //Load the actions from the template
            Engine = engine;
            if ((null != engine) && !String.IsNullOrEmpty(FileName?.File))
            {
                var templateContainerModel = new TemplateContainerModel(this.FileName);
                templateContainerModel.ReadXmlFile(content);
                StateActionsList.LoadStateActions(templateContainerModel.StatesActions, this.Owner, this.Owner.States);
            }

            StateActionsList.LoadContent(engine, content);
        }

        #endregion //Initialization

        #region Methods

        public override void Reset()
        {
            for (var i = 0; i < StateActionsList.Actions.Count; i++)
            {
                StateActionsList.Actions[i].Reset();
            }
            base.Reset();
        }

        public override bool Execute(float currentTime)
        {
            if (StateActionsList.ExecuteAction(currentTime))
            {
                return true;
            }

            //Check if all the actions are done
            AlreadyRun = true;
            foreach (var action in StateActionsList.Actions)
            {
                if (!action.AlreadyRun)
                {
                    AlreadyRun = false;
                    break;
                }
            }

            return false;
        }

        public virtual void Update()
        {
            //nothing to do here, used in child classes
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