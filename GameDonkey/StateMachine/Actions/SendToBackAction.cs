using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
    public class SendToBackAction : BaseAction
    {
        #region Initialization

        public SendToBackAction(BaseObject owner) :
            base(owner, EActionType.SendToBack)
        {
        }

        public SendToBackAction(BaseObject owner, SendToBackActionModel actionModel) :
            base(owner, actionModel)
        {
        }

        public SendToBackAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as SendToBackActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
        }

        #endregion //Initialization

        #region Methods
        public override bool Execute(float currentTime)
        {
            //get that dude's characterqeueu and deactiuvate it
            Owner.PlayerQueue.SendToBack(Owner);

            //never set these actions to already run
            AlreadyRun = false;
            return true;
        }

        #endregion //Methods
    }
}