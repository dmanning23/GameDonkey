using AnimationLib;
using FilenameBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;

namespace GameDonkeyLib
{
	public class ProjectileAction : BaseAction
	{
		#region Properties

		/// <summary>
		/// the projectile to add
		/// </summary>
		private BaseObject Projectile;

		/// <summary>
		/// the filename of the projectile data.xml file to use
		/// </summary>
		public Filename FileName { get; set; }

		/// <summary>
		/// the offset from the bone to start the particle effect from
		/// this is ignored if the bone thing is set
		/// </summary>
		public Vector2 StartOffset { get; set; }

		/// <summary>
		/// The direction to set the projectile's initial velocity when this action is run.
		/// This is only used if the thumbstick flag is false
		/// </summary>
		public ActionDirection Velocity { get; set; }

		/// <summary>
		/// How much to scale the projectile.  Read in from the xml file, not the "runtime scale"
		/// </summary>
		private float _scale;
		public float Scale
		{
			get { return _scale; }
			set
			{
				_scale = value;
				if (null != Projectile)
				{
					Projectile.Scale = Scale * Owner.Scale;
				}
			}
		}

		#endregion //Properties

		#region Initialization

		public ProjectileAction(BaseObject owner) :
			base(owner, EActionType.Projectile)
		{
			Velocity = new ActionDirection();
			FileName = new Filename();
			StartOffset = Vector2.Zero;
			Scale = 1f;
		}

		public ProjectileAction(BaseObject owner, ProjectileActionModel actionModel) :
			base(owner, actionModel)
		{
			Velocity = new ActionDirection(actionModel.Direction);
			FileName = new Filename(actionModel.Filename);
			StartOffset = new Vector2(actionModel.StartOffset.X, actionModel.StartOffset.Y);
			Scale = actionModel.Scale;
		}

		public ProjectileAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as ProjectileActionModel)
		{
		}

		public override void LoadContent(IGameDonkey engine, IStateContainer stateContainer, ContentManager content)
		{
			//try to load the file into the particle effect
			if ((null != engine) && !String.IsNullOrEmpty(FileName.File))
			{
				//load object into player queue!
				Projectile = Owner.PlayerQueue.LoadXmlObject(FileName, engine, GameObjectType.Projectile, 0, content);
				Projectile.Scale = Scale * Owner.Scale;
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
			if (null == Projectile)
			{
				//boo... you need to have a projectile loaded
				return true;
			}

			//activate the projectile
			bool bActivated = Owner.PlayerQueue.ActivateObject(Projectile);

			//if it was activated (wont be activated if already active)
			if (bActivated)
			{
				//get the start position for the projectile
				var ProjectilePosition = StartOffset * Projectile.Scale;
				ProjectilePosition.Y = Owner.Position.Y + ProjectilePosition.Y;
				ProjectilePosition.X = Owner.Position.X + (Owner.Flip ? -ProjectilePosition.X : ProjectilePosition.X);

				//set the position
				Projectile.Position = ProjectilePosition;
				Projectile.Flip = Owner.Flip;

				Projectile.Velocity = (Velocity.GetDirection(Owner) / Owner.Scale) * Projectile.Scale;

				//run the animation container so all the bones will be in the correct position when it updates
				//This way, any particle effects created will be in correct location.
				Projectile.AnimationContainer.SetAnimation(0, EPlayback.Loop);
				Projectile.AnimationContainer.Update(Owner.PlayerQueue.CharacterClock,
					Projectile.Position,
					Projectile.Flip,
					0.0f,
					true);
			}

			return base.Execute();
		}

		#endregion //Methods
	}
}