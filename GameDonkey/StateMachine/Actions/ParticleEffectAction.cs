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
		public EmitterTemplate Emitter { get; set; }
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
		public Bone Bone { get; private set; }
		public ActionDirection Velocity { get; set; }
		public Vector2 StartOffset { get; set; }

		private ParticleEngine ParticleEngine { get; set; }
		public bool UseBoneRotation { get; set; }
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

		public void UpdateParticleEffectColor(Color color)
		{
			Emitter.ParticleColor = new Color(color.R, color.G, color.B, Emitter.ParticleColor.A);
		}

		#endregion //Methods
	}
}