using GameTimer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;
using System;
using System.Diagnostics;

namespace GameDonkeyLib
{
	public class LevelObject : BaseObject
	{
		#region Members

		/// <summary>
		/// how fast players will pop out of a level object, pixels/second
		/// </summary>
		private const float MoveSpeed = 1750.0f;

		#endregion //Members

		#region Methods

		public LevelObject(HitPauseClock clock, int queueID) : base(GameObjectType.Level, clock, queueID)
		{
		}

		protected override void Init()
		{
			Physics = new LevelObjectPhysicsContainer(this);
			States = new ObjectStateContainer(new HybridStateMachine());
			States.StateChangedEvent += this.StateChanged;
		}

		public override void KillPlayer()
		{
		}

		public override void CollisionResponse(BasePhysicsContainer otherObject,
			CreateAttackAction attackAction,
			Vector2 firstCollisionPoint,
			Vector2 secondCollisionPoint)
		{
			//get a vector from the level object to the object
			var levelToObject = firstCollisionPoint - secondCollisionPoint;

			//set how far to move the other object
			var moveSpeed = levelToObject.Length();
			if (moveSpeed <= 0.0f)
			{
				return;
			}

			if (levelToObject.Y > 0.0f)
			{
				//set the distance to diameter of the circle minus the current y
				moveSpeed += (CharacterClock.TimeDelta * MoveSpeed);
			}
			//if (fMoveSpeed > (CharacterClock.TimeDelta * m_fMoveSpeed))
			//{
			//    fMoveSpeed = (CharacterClock.TimeDelta * m_fMoveSpeed);
			//}

			//add a "ground hit" to the other object?
			levelToObject.Y = -1.0f * Math.Abs(levelToObject.Y);
			levelToObject.Normalize();
			if (!otherObject.HitFlags[(int)EHitType.GroundHit] || (moveSpeed > otherObject.Hits[(int)EHitType.GroundHit].Strength))
			{
				otherObject.HitFlags[(int)EHitType.GroundHit] = true;
				otherObject.Hits[(int)EHitType.GroundHit].Set(
					levelToObject,
					null,
					moveSpeed,
					EHitType.GroundHit,
					null,
					firstCollisionPoint);
			}
		}

		protected override void RespondToGroundHit(Hit groundHit, IGameDonkey engine)
		{
			//should never get here
			Debug.Assert(false);
		}
		
		#endregion //Methods

		#region File IO

		/// <summary>
		/// Given an xml node, parse the contents.
		/// Override in child classes to read object-specific node types.
		/// </summary>
		/// <param name="childNode">the xml data to read</param>
		/// <param name="engine">the engine we are using to load</param>
		/// <param name="messageOffset">the message offset of this object's state machine</param>
		/// <returns></returns>
		public override void ParseXmlData(BaseObjectModel model, IGameDonkey engine, ContentManager content = null)
		{
			var data = model as LevelObjectModel;
			if (null == data)
			{
				throw new Exception("must pass LevelObjectModel to LevelObject.ParseXmlData");
			}

			//set the scale
			Scale = data.Size;
		
			//set teh position
			Position = data.Position;

			base.ParseXmlData(model, engine, content);
		}

		#endregion //File IO
	}
}