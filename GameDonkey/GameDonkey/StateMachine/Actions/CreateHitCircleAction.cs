using AnimationLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	/// <summary>
	/// This is an attack action that uses a unattached circle instead of a bone
	/// </summary>
	public class CreateHitCircleAction : CreateAttackAction
	{
		#region Properties

		/// <summary>
		/// this dudes hit circle that will be floating around
		/// </summary>
		protected PhysicsCircle HitCircle;

		public float Radius
		{
			get
			{
				return HitCircle.Radius;
			}
			set
			{
				HitCircle.Radius = value;
			}
		}

		/// <summary>
		/// the offset from the attached bone location to start this circle at
		/// </summary>
		public Vector2 StartOffset { get; set; }

		/// <summary>
		/// speed and direction of this circle
		/// </summary>
		public Vector2 Velocity { get; set; }

		#endregion //Properties

		#region Initialization

		public CreateHitCircleAction(BaseObject owner) :
			base(owner, EActionType.CreateHitCircle)
		{
			HitCircle = new PhysicsCircle();
			StartOffset = Vector2.Zero;
			Velocity = Vector2.Zero;
		}

		public CreateHitCircleAction(BaseObject owner, CreateHitCircleActionModel actionModel, IStateContainer container) :
			base(owner, actionModel, container)
		{
			HitCircle = new PhysicsCircle()
			{
				Radius = actionModel.Radius,
			};
			StartOffset = actionModel.StartOffset;
			Velocity = actionModel.Velocity;
		}

		public CreateHitCircleAction(BaseObject owner, BaseActionModel actionModel, IStateContainer container) :
			this(owner, actionModel as CreateHitCircleActionModel, container)
		{
		}

		public override void LoadContent(IGameDonkey engine, ContentManager content)
		{
			base.LoadContent(engine, content);
		}

		#endregion //Initialization

		#region Methods

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			SetAttackBone();

			//set the circle location

			//get the bone location
			var myLocation = AttackBone.AnchorPosition;

			//get the start offset
			var myOffset = StartOffset;
			if (Owner.Flip)
			{
				myOffset.X *= -1.0f;
			}

			//set the circle location
			HitCircle.Reset(myLocation - myOffset);

			return base.Execute();
		}

		public override void Update()
		{
			//add the velocity
			var myPosition = HitCircle.Pos + ((Velocity * Owner.Scale) * Owner.CharacterClock.TimeDelta);

			//update the circle location
			HitCircle.Update(myPosition);
		}

		public override PhysicsCircle GetCircle()
		{
			return HitCircle;
		}

		#endregion //Methods
	}
}