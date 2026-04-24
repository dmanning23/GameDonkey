using CameraBuddy;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class CameraShakeAction : BaseAction
	{
		#region Properties
		public float TimeDelta { get; set; }
		public float ShakeAmount { get; set; }
		public ICamera Camera { get; set; }

		#endregion //Properties

		#region Initialization

		public CameraShakeAction(BaseObject owner) :
			base(owner, EActionType.CameraShake)
		{
			TimeDelta = 0.25f;
			ShakeAmount = 1.0f;
		}

		public CameraShakeAction(BaseObject owner, CameraShakeActionModel actionModel) :
			base(owner, actionModel)
		{
			TimeDelta = actionModel.TimeDelta.TimeDelta;
			ShakeAmount = actionModel.ShakeAmount;
		}

		public CameraShakeAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as CameraShakeActionModel)
		{
		}

		public override void LoadContent(IGameDonkey engine, ContentManager content)
		{
			Camera = engine.Renderer.Camera;
		}

		#endregion //Initialization

		#region Methods
		public override bool Execute()
		{
			Camera.AddCameraShake(TimeDelta, ShakeAmount);

			return base.Execute();
		}

		#endregion //Methods
	}
}