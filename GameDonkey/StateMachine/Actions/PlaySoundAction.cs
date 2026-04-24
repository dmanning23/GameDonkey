using FilenameBuddy;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class PlaySoundAction : BaseAction
	{
		#region Properties

		IGameDonkey _engine;

		private Filename _soundCueName;
		public Filename SoundCueName
		{
			get
			{
				return _soundCueName;
			}
			set
			{
				_soundCueName = value;
				if (null != _engine && 
					!_engine.ToolMode &&
					!string.IsNullOrEmpty(SoundCueName.File))
				{
					Sound = _engine.LoadSound(SoundCueName);
				}
			}
		}
		public SoundEffect Sound { get; private set; }

		#endregion //Properties

		#region Initialization

		public PlaySoundAction(BaseObject owner) :
			base(owner, EActionType.PlaySound)
		{
			SoundCueName = new Filename();
		}

		public PlaySoundAction(BaseObject owner, PlaySoundActionModel actionModel) :
			base(owner, actionModel)
		{
			SoundCueName = new Filename(actionModel.Filename);
		}

		public PlaySoundAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as PlaySoundActionModel)
		{
		}

		public override void LoadContent(IGameDonkey engine, ContentManager content)
		{
			_engine = engine;
			if (!string.IsNullOrEmpty(SoundCueName.File) && !_engine.ToolMode)
			{
				Sound = engine.LoadSound(SoundCueName);
			}
		}

		#endregion //Initialization

		#region Methods
		public override bool Execute()
		{
			//execute sound action
			if (null != Sound)
			{
				Sound.Play();
			}

			return base.Execute();
		}

		#endregion //Methods
	}
}