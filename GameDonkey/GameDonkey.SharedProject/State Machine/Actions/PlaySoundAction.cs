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

		/// <summary>
		/// the filename of the sound file to use
		/// </summary>		/// <value>The name of the sound cue.</value>
		public Filename SoundCueName
		{
			get
			{
				return _soundCueName;
			}
			set
			{
				_soundCueName = value;
				if (null != _engine && !string.IsNullOrEmpty(SoundCueName.File))
				{
					Sound = _engine.LoadSound(SoundCueName);
				}
			}
		}

		/// <summary>
		/// Gets the sound.
		/// </summary>
		/// <value>The sound.</value>
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

		public override void LoadContent(IGameDonkey engine, IStateContainer stateContainer, ContentManager content)
		{
			_engine = engine;
			if (!string.IsNullOrEmpty(SoundCueName.File))
			{
				Sound = engine.LoadSound(SoundCueName);
			}
		}

		#endregion //Initialization

		#region Methods

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
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