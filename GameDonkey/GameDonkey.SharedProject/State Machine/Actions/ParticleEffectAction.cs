using AnimationLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ParticleBuddy;
using System;

namespace GameDonkeyLib
{
	public class ParticleEffectAction : BaseAction
	{
		#region Properties

		/// <summary>
		/// the particle effect template to use
		/// </summary>
		public EmitterTemplate Emitter { get; set; }

		/// <summary>
		/// the name of the bone to emanate from
		/// </summary>
		private string _boneName;
		public string BoneName
		{
			get { return _boneName; }
			set
			{
				_boneName = value;
				if (String.IsNullOrEmpty(_boneName) || null == Owner)
				{
					Bone = null;
				}
				else
				{
					Bone = Owner.AnimationContainer.Skeleton.RootBone.GetBone(_boneName);
				}
			}
		}

		/// <summary>
		/// the bone to attach the particle emitter to
		/// </summary>
		public Bone Bone { get; private set; }

		/// <summary>
		/// the direction to shoot the particle effect
		/// </summary>
		public ActionDirection Velocity { get; set; }

		/// <summary>
		/// the offset from the character origin to start the particle effect from
		/// ignored if the source bone is set
		/// </summary>
		public Vector2 StartOffset { get; set; }

		private ParticleEngine ParticleEngine { get; set; }

		/// <summary>
		/// When a particle is fired, whether or not it should match the rotation of the specified bone.
		/// </summary>
		public bool UseBoneRotation { get; set; }

		/// <summary>
		/// Flag to use the player's color instead of the emitter color
		/// </summary>
		public bool UsePlayerColor { get; set; }

		#endregion //Properties

		#region Initialization

		public ParticleEffectAction(BaseObject owner) :
			base(owner, EActionType.ParticleEffect)
		{
			Emitter = new EmitterTemplate();
			BoneName = "";
			Velocity = new ActionDirection();
			StartOffset = Vector2.Zero;
			UseBoneRotation = false;
			UsePlayerColor = false;
		}

		public ParticleEffectAction(BaseObject owner, ParticleEffectActionModel actionModel) :
			base(owner, actionModel)
		{
			Emitter = new EmitterTemplate(actionModel.Emitter);
			BoneName = actionModel.Bone;
			Velocity = new ActionDirection(actionModel.Direction);
			StartOffset = actionModel.StartOffset;
			UseBoneRotation = actionModel.UseBoneRotation;
			UsePlayerColor = actionModel.UsePlayerColor;
		}

		public ParticleEffectAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as ParticleEffectActionModel)
		{
		}

		public override void LoadContent(IGameDonkey engine, ContentManager content)
		{
			ParticleEngine = engine.ParticleEngine;
			Emitter.LoadContent(engine.Renderer);
		}

		#endregion //Initialization

		#region Methods

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			var emitter = ParticleEngine.PlayParticleEffect(
				Emitter,
				Velocity.GetDirection(Owner),
				Owner.Position,
				StartOffset,
				GetColor(),
				GetFlip(),
				GetPosDelegate(),
				GetRotationDelegate(),
				GetOwnerRotation());

			if (null != emitter)
			{
				Owner.Emitters.Add(emitter);
			}

			return base.Execute();
		}

		private Color GetColor()
		{
			return UsePlayerColor ? Owner.PlayerColor : Emitter.ParticleColor;
		}

		private PositionDelegate GetPosDelegate()
		{
			if (null != Bone)
			{
				return Bone.GetPosition;
			}

			return null;
		}

		private RotationDelegate GetRotationDelegate()
		{
			if ((null != Bone) && UseBoneRotation)
			{
				return Bone.TrueRotationAngle;
			}

			return null;
		}

		private bool GetFlip()
		{
			if ((null != Bone) && UseBoneRotation)
			{
				return Bone.Flipped;
			}

			return Owner.Flip;
		}

		private RotationDelegate GetOwnerRotation()
		{
			if ((null != Bone) && UseBoneRotation)
			{
				return null;
			}

			return Owner.Rotation;
		}

		#endregion //Methods
	}
}