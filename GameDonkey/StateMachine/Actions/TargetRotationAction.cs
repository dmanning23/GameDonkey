using Microsoft.Xna.Framework.Content;
using Vector2Extensions;

namespace GameDonkeyLib
{
    public class TargetRotationAction : TimedAction
    {
        #region Properties
        public ActionDirection TargetRotation { get; set; }

        #endregion //Properties

        #region Initialization

        public TargetRotationAction(BaseObject owner) :
            base(owner, EActionType.TargetRotation)
        {
            TargetRotation = new ActionDirection();
        }

        public TargetRotationAction(BaseObject owner, TargetRotationActionModel actionModel) :
            base(owner, actionModel, actionModel.TimeDelta)
        {
            TargetRotation = new ActionDirection(actionModel.Direction);
        }

        public TargetRotationAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as TargetRotationActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
        }

        #endregion //Initialization

        #region Methods
        public override bool Execute(float currentTime)
        {
            //get the direction
            var direction = TargetRotation.GetDirection(Owner);
            direction.X = (Owner.Flip ? (direction.X * -1.0f) : direction.X);

            //Convert the direction to a rotation
            var angle = Helper.ClampAngle(direction.Angle());

            //get the amount of rotation/second to add
            var rotationDelta = 0.0f;
            if (Owner.Flip)
            {
                rotationDelta = angle + Owner.CurrentRotation;
            }
            else
            {
                rotationDelta = angle - Owner.CurrentRotation;
            }

            //change to rotation / second
            rotationDelta /= TimeDelta;

            //set the rotation action variable in the base object
            Owner.RotationPerSecond = rotationDelta;

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}