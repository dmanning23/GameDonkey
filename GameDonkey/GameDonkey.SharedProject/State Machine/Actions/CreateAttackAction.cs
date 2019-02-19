using AnimationLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace GameDonkeyLib
{
	public class CreateAttackAction : TimedAction
	{
		#region Properties

		/// <summary>
		/// the name of the bone to use
		/// </summary>
		private string _boneName;
		public string BoneName
		{
			get { return _boneName; }
			set
			{
				_boneName = value;

				//if the bone name is changed, it means the bone needs to be reset too...
				AttackBone = null;
			}
		}

		/// <summary>
		/// The bone this attack uses.  Has to be a weapon bone!
		/// Starts out as null, and is set at runtime the first time this action is run.
		/// Since it can be a garment bone, these might not actually be in the model at startup
		/// </summary>
		public Bone AttackBone { get; protected set; }

		/// <summary>
		/// the vector to set another object to when this attack connects
		/// </summary>
		public ActionDirection ActionDirection { get; set; }
		public Vector2 Direction
		{
			get
			{
				return ActionDirection.GetDirection(Owner);
			}
		}

		public SoundEffect HitSound { get; private set; }

		/// <summary>
		/// A list of actions that will be run if this attack connects (sound effects, particle effects, etc)
		/// This list of actions is played whether the attack is blocked or not.
		/// </summary>
		public List<BaseAction> SuccessActions { get; private set; }

		/// <summary>
		/// the amount of damage to deal when this attack connects
		/// </summary>
		public float Damage { get; set; }

		#endregion //Properties

		#region Initialization

		public CreateAttackAction(BaseObject owner, EActionType actionType = EActionType.CreateAttack) :
			base(owner, actionType)
		{
			ActionDirection = new ActionDirection();
			SuccessActions = new List<BaseAction>();
		}

		public CreateAttackAction(BaseObject owner, CreateAttackActionModel actionModel) :
			base(owner, actionModel, actionModel.TimeDelta)
		{
			BoneName = actionModel.BoneName;
			Damage = actionModel.Damage;
			ActionDirection = new ActionDirection(actionModel.Direction);
			SuccessActions = new List<BaseAction>();
			for (int i = 0; i < actionModel.SuccessActions.Count; i++)
			{
				var stateAction = StateActionFactory.CreateStateAction(actionModel.SuccessActions[i], owner);
				SuccessActions.Add(stateAction);
			}
		}

		public CreateAttackAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as CreateAttackActionModel)
		{
		}

		public override void LoadContent(IGameDonkey engine, ContentManager content)
		{
			for (int i = 0; i < SuccessActions.Count; i++)
			{
				SuccessActions[i].LoadContent(engine, content);
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
			//Check if the bone is set, if not try and find it...
			if (null == AttackBone)
			{
				AttackBone = Owner.Physics.FindWeapon(BoneName);
			}

			//reset teh success actions
			for (var i = 0; i < SuccessActions.Count; i++)
			{
				SuccessActions[i].AlreadyRun = false;
			}

			//activate the attack
			DoneTime = Owner.CharacterClock.CurrentTime + TimeDelta;

			//add this actionto the list of attacks
			Owner.AddAttack(this);

			return base.Execute();
		}

		public virtual void Update()
		{
			//nothing to do here, used in child classes
		}

		public virtual PhysicsCircle GetCircle()
		{
			//return the first circle from this dude's image

			//Check if the bone is set, if not try and find it...
			if (null == AttackBone)
			{
				AttackBone = Owner.Physics.FindWeapon(BoneName);
			}

			//the bone for this attack is in a garment that isnt being displayed
			if (null == AttackBone)
			{
				return null;
			}

			//get the current image
			var image = AttackBone.GetCurrentImage();

			//hit bones and images must have one circle
			if ((null == image) || (image.Circles.Count < 1))
			{
				return null;
			}

			//get the circle
			var circle = image.Circles[0];

			return circle;
		}

		/// <summary>
		/// execute all the success actions after this attack lands
		/// </summary>
		/// <param name="characterHit">The dude that got nailed by this attack</param>
		/// <returns>bool: whether or not a state change occurred while this dude was running</returns>
		public virtual bool ExecuteSuccessActions(BaseObject characterHit)
		{
			var result = false;
			for (var i = 0; i < SuccessActions.Count; i++)
			{
				if (SuccessActions[i].Execute())
				{
					result = true;
				}
			}

			return result;
		}

		#endregion //Methods
	}
}