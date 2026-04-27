using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
    public class EvadeAction : TimedAction
    {
        #region Initialization

        public EvadeAction(BaseObject owner) :
            base(owner, EActionType.Evade)
        {
        }

        public EvadeAction(BaseObject owner, EvadeActionModel actionModel) :
            base(owner, actionModel, actionModel.TimeDelta)
        {
        }

        public EvadeAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as EvadeActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
        }

        #endregion //Initialization

        #region Methods
        public override bool Execute(float currentTime)
        {
            //activate the attack
            Owner.EvasionTimer.Start(TimeDelta);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}