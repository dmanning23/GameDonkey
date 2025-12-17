using MatrixExtensions;
using Microsoft.Xna.Framework;

namespace GameDonkeyLib
{
	/// <summary>
	/// This is a class to wrap up getting a direction for directional actions.
	/// </summary>
	public class ActionDirection
	{
		#region Properties

		/// <summary>
		/// Whether or not we want this action to use the left thumbstick to get its direction.
		/// </summary>
		public EDirectionType DirectionType { get; set; }

		/// <summary>
		/// The direction to use
		/// </summary>
		private Vector2 _velocity;

		/// <summary>
		/// The length of the velocity to add to the character.
		/// This is only used if the thumbstick flag is true
		/// </summary>
		private float _velocityLength;

		public Vector2 Velocity
		{
			get { return _velocity; }
			set
			{
				_velocity = value;
				_velocityLength = _velocity.Length();
			}
		}

		public float VelocityLength
		{
			get { return _velocityLength; }
		}

		#endregion //Properties

		#region Initialization

		/// <summary>
		/// Standard constructor
		/// </summary>
		public ActionDirection()
		{
			Velocity = new Vector2(0.0f);
			DirectionType = EDirectionType.Absolute;
		}

		/// <summary>
		/// Standard constructor
		/// </summary>
		public ActionDirection(DirectionActionModel direction)
		{
			Velocity = new Vector2(direction.Velocity.X, direction.Velocity.Y); ;
			DirectionType = direction.DirectionType;
		}

		#endregion //Initialization

		#region Methods

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public Vector2 GetDirection(BaseObject owner)
		{
			switch (DirectionType)
			{
				case EDirectionType.Controller:
					{
						return ControllerDirection(owner) * owner.Scale;
					}
				case EDirectionType.ControllerOrVelocity:
					{
						return ControllerOrVelocityDirection(owner) * owner.Scale;
					}
				case EDirectionType.NegController:
					{
						return NegControllerDirection(owner) * owner.Scale;
					}
				case EDirectionType.NegControllerOrVelocity:
					{
						return NegControllerOrVelocityDirection(owner) * owner.Scale;
					}
				case EDirectionType.Velocity:
					{
						return VelocityDirection(owner) * owner.Scale;
					}
				case EDirectionType.Relative:
					{
						return RelativeDirection(owner) * owner.Scale;
					}
				default: //absolute
					{
						return AbsoluteDirection(owner) * owner.Scale;
					}
			}
		}

		private Vector2 ControllerDirection(BaseObject owner)
		{
			if (owner.Direction() != Vector2.Zero)
			{
				//use the thumbstick direction from the object
				return owner.Direction() * VelocityLength;
			}
			else
			{
				//use the velocity from this action
				return RelativeDirection(owner);
			}
		}

		private Vector2 ControllerOrVelocityDirection(BaseObject owner)
		{
			if (owner.Direction() != Vector2.Zero)
			{
				//use the thumbstick direction from the object
				return owner.Direction() * VelocityLength;
			}
			else if (owner.Velocity != Vector2.Zero)
			{
				//use teh direction the object is travelling
				return VelocityDirection(owner);
			}
			else
			{
				//use the velocity from this action
				return RelativeDirection(owner);
			}
		}

		private Vector2 NegControllerDirection(BaseObject owner)
		{
			if (owner.Direction() != Vector2.Zero)
			{
				//use the opposite thumbstick direction from the object
				return owner.Direction() * -VelocityLength;
			}
			else
			{
				//use the velocity from this action
				return -1.0f * RelativeDirection(owner);
			}
		}

		private Vector2 NegControllerOrVelocityDirection(BaseObject owner)
		{
			if (owner.Direction() != Vector2.Zero)
			{
				//use the opposite thumbstick direction from the object
				return owner.Direction() * -VelocityLength;
			}
			else if (owner.Velocity != Vector2.Zero)
			{
				//use teh direction the object is travelling, but flip the X
				return -1.0f * VelocityDirection(owner);
			}
			else
			{
				//use the velocity from this action
				return -1.0f * RelativeDirection(owner);
			}
		}

		private Vector2 VelocityDirection(BaseObject owner)
		{
			if (owner.Velocity != Vector2.Zero)
			{
				//use teh direction the object is travelling
				var direction = owner.Velocity;
				direction.Normalize();
				return direction * VelocityLength;
			}
			else
			{
				//use teh direction based on where the character is pointing
				return RelativeDirection(owner);
			}
		}

		private Vector2 RelativeDirection(BaseObject owner)
		{
			//use teh direction based on where the character is pointing
			var direction = Velocity;
			var rotation = MatrixExt.Orientation(owner.CurrentRotation);
			direction.X = (owner.Flip ? -Velocity.X : Velocity.X);
			return MatrixExt.Multiply(rotation, direction);
		}

		private Vector2 AbsoluteDirection(BaseObject owner)
		{
			//use the velocity from this action
			var direction = Velocity;
			direction.X = (owner.Flip ? -Velocity.X : Velocity.X);
			return direction;
		}

		#endregion //Methods
	}
}