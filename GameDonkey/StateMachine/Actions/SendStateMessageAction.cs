using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
    public class SendStateMessageAction : BaseAction
    {
        #region Properties
        public string Message { get; set; }
        IStateContainer StateContainer { get; set; }

        #endregion //Properties

        #region Initialization

        public SendStateMessageAction(BaseObject owner) :
            base(owner, EActionType.SendStateMessage)
        {
        }

        public SendStateMessageAction(BaseObject owner, SendStateMessageActionModel actionModel, IStateContainer stateContainer) :
            base(owner, actionModel)
        {
            Message = actionModel.Message;
            StateContainer = stateContainer;
        }

        public SendStateMessageAction(BaseObject owner, BaseActionModel actionModel, IStateContainer stateContainer) :
            this(owner, actionModel as SendStateMessageActionModel, stateContainer)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
        }

        #endregion //Initialization

        #region Methods
        public override bool Execute(float currentTime)
        {
            //The message offset is added to this message when it is read in, so dont add anything
            StateContainer.SendStateMessage(Message);

            //keep running the action until it goes through?
            AlreadyRun = false;
            return true;
        }

        #endregion //Methods
    }
}