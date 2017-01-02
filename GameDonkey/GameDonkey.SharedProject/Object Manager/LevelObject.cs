using FilenameBuddy;
using GameTimer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;
using System;
using System.Diagnostics;

namespace GameDonkey
{
	public class LevelObject : BaseObject
	{
		#region Members

		/// <summary>
		/// how fast players will pop out of a level object, pixels/second
		/// </summary>
		private const float m_fMoveSpeed = 1750.0f;

		#endregion //Members

		#region Methods

		public LevelObject(HitPauseClock rClock, int iQueueID) : base(EObjectType.Level, rClock, iQueueID)
		{
		}

		protected override void Init()
		{
			Physics = new LevelObjectPhysicsContainer(this);
			States = new ObjectStateContainer(new StateMachine());
			States.StateChangedEvent += this.StateChanged;
		}

		public override void CollisionResponse(IPhysicsContainer rOtherObject,
			CreateAttackAction rAttackAction,
			Vector2 FirstCollisionPoint,
			Vector2 SecondCollisionPoint)
		{
			Debug.Assert(null != Physics);
			Debug.Assert(null != rOtherObject);

			//get a vector from the level object to the object
			Vector2 LevelToObject = FirstCollisionPoint - SecondCollisionPoint;

			//set how far to move the other object
			float fMoveSpeed = LevelToObject.Length();
			if (fMoveSpeed <= 0.0f)
			{
				return;
			}

			if (LevelToObject.Y > 0.0f)
			{
				//set the distance to diameter of the circle minus the current y
				fMoveSpeed += (CharacterClock.TimeDelta * m_fMoveSpeed);
			}
			//if (fMoveSpeed > (CharacterClock.TimeDelta * m_fMoveSpeed))
			//{
			//    fMoveSpeed = (CharacterClock.TimeDelta * m_fMoveSpeed);
			//}

			//add a "ground hit" to the other object?
			LevelToObject.Y = -1.0f * Math.Abs(LevelToObject.Y);
			LevelToObject.Normalize();
			if (!rOtherObject.HitFlags[(int)EHitType.GroundHit] || (fMoveSpeed > rOtherObject.Hits[(int)EHitType.GroundHit].Strength))
			{
				Debug.Assert(null != rOtherObject.Hits[(int)EHitType.GroundHit]);

				rOtherObject.HitFlags[(int)EHitType.GroundHit] = true;
				rOtherObject.Hits[(int)EHitType.GroundHit].Set(
					LevelToObject,
					null,
					fMoveSpeed,
					EHitType.GroundHit,
					null,
					FirstCollisionPoint);
			}
		}

		protected override void RespondToGroundHit(Hit rGroundHit, IGameDonkey rEngine)
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
		/// <param name="rEngine">the engine we are using to load</param>
		/// <param name="iMessageOffset">the message offset of this object's state machine</param>
		/// <returns></returns>
		public override bool ParseXmlData(BaseObjectData childNode, IGameDonkey rEngine, int iMessageOffset, ContentManager content = null)
		{
			var data = childNode as LevelObjectData;
			if (null == data)
			{
				return false;
			}

			//set the scale
			Scale = data.Size;
		
			//set teh position
			Position = data.Position;

			return base.ParseXmlData(childNode, rEngine, iMessageOffset, content);
		}

		#endregion //File IO
	}
}