using AnimationLib;
using FilenameBuddy;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
    public class AddGarmentAction : TimedAction
    {
        #region Properties
        public Garment Garment { get; private set; }

        public Filename Filename { get; set; }

        #endregion //Properties

        #region Initialization

        public AddGarmentAction(BaseObject owner) :
            base(owner, EActionType.AddGarment)
        {
            Filename = new Filename();
        }

        public AddGarmentAction(BaseObject owner, AddGarmentActionModel actionModel) :
            base(owner, actionModel, actionModel.TimeDelta)
        {
            Filename = new Filename(actionModel.Filename);
        }

        public AddGarmentAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as AddGarmentActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            //load the garment from the garment manager
            Garment = Owner.Garments.LoadGarment(Filename, engine.Renderer, content);
        }

        #endregion //Initialization

        #region Methods
        public override bool Execute(float currentTime)
        {
            //add this actionto the list of garments
            Owner.Garments.AddAction(this, Owner.CharacterClock);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}