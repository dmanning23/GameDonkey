using CameraBuddy;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class CameraShakeAction : BaseAction
	{
		#region Properties

		/// <summary>
		/// the length of time to shake the camera
		/// </summary>
		public float TimeDelta { get; set; }

		/// <summary>
		/// how hard to shake the camera
		/// </summary>
		public float ShakeAmount { get; set; }

		/// <summary>
		/// the camera to shake
		/// </summary>
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

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			Camera.AddCameraShake(TimeDelta, ShakeAmount);

			return base.Execute();
		}

		#endregion //Methods
	}
}