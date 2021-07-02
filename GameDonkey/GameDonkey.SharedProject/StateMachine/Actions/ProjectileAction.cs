using AnimationLib;
using FilenameBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Linq;

namespace GameDonkeyLib
{
	public class ProjectileAction : BaseAction
	{
		#region Properties

		protected ProjectileObjectModel ProjectileObjectModel { get; set; }

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
		public float Scale { get; set; }

		private IGameDonkey Engine { get; set; }

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

		public override void LoadContent(IGameDonkey engine, ContentManager content)
		{
			Engine = engine;

			//try to load the file into the particle effect
			if ((null != engine) && !String.IsNullOrEmpty(FileName.File))
			{
				//load object into player queue!
				ProjectileObjectModel = new ProjectileObjectModel(FileName);
				ProjectileObjectModel.ReadXmlFile(content);
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
			//load the character into the playerqueue
			ProjectileObject projectile = null;
			if (!Engine.ToolMode)
			{
				using (var xmlContent = new ContentManager(Engine.Game.Services, "Content"))
				{
					projectile = Owner.PlayerQueue.LoadXmlObject(ProjectileObjectModel, Engine, "Projectile", xmlContent) as ProjectileObject;
				}
			}
			else
			{
				projectile = Owner.PlayerQueue.LoadXmlObject(ProjectileObjectModel, Engine, "Projectile", null) as ProjectileObject;
			}

			Owner.PlayerQueue.ActivateObject(projectile);

			projectile.Scale = Scale * Owner.Scale;

			//get the start position for the projectile
			var ProjectilePosition = StartOffset * projectile.Scale;
			ProjectilePosition.Y = Owner.Position.Y + ProjectilePosition.Y;
			ProjectilePosition.X = Owner.Position.X + (Owner.Flip ? -ProjectilePosition.X : ProjectilePosition.X);

			//set the position
			projectile.Position = ProjectilePosition;
			projectile.Flip = Owner.Flip;

			projectile.Velocity = (Velocity.GetDirection(Owner) / Owner.Scale) * projectile.Scale;

			//run the animation container so all the bones will be in the correct position when it updates
			//This way, any particle effects created will be in correct location.
			var animationName = projectile.AnimationContainer.Animations.First().Key;
			projectile.AnimationContainer.SetAnimation(animationName, EPlayback.Loop);
			projectile.AnimationContainer.Update(Owner.PlayerQueue.CharacterClock,
					projectile.Position,
					projectile.Flip,
					0f,
					true);

			return base.Execute();
		}

		#endregion //Methods
	}
}