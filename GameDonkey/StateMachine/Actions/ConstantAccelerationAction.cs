using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
    public class ConstantAccelerationAction : BaseAction
    {
        #region Properties
        public ActionDirection Velocity { get; set; }
        public float MaxVelocity { get; set; }

        #endregion //Properties

        #region Initialization

        public ConstantAccelerationAction(BaseObject owner) :
            base(owner, EActionType.ConstantAcceleration)
        {
            Velocity = new ActionDirection();
        }

        public ConstantAccelerationAction(BaseObject owner, ConstantAccelerationActionModel actionModel) :
            base(owner, actionModel)
        {
            Velocity = new ActionDirection(actionModel.Direction);
            MaxVelocity = actionModel.MaxVelocity;
        }

        public ConstantAccelerationAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as ConstantAccelerationActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
        }

        #endregion //Initialization

        #region Methods
        public override bool Execute(float currentTime)
        {
            //set the constant accleration variable in the base object
            Owner.AccelAction = this;

            return base.Execute(currentTime);
        }

        public Vector2 GetVelocity()
        {
            return Velocity.GetDirection(Owner);
        }

        #endregion //Methods
    }
}