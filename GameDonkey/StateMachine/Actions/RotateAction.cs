using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
    public class RotateAction : BaseAction
    {
        #region Properties
        public float Rotation { get; set; }

        #endregion //Properties

        #region Initialization

        public RotateAction(BaseObject owner) :
            base(owner, EActionType.Rotate)
        {
        }

        public RotateAction(BaseObject owner, RotateActionModel actionModel) :
            base(owner, actionModel)
        {
            Rotation = actionModel.Rotation;
        }

        public RotateAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as RotateActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
        }

        #endregion //Initialization

        #region Methods
        public override bool Execute(float currentTime)
        {
            //set the rotation action variable in the base object
            Owner.RotationPerSecond = Rotation;

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}