using StateMachineBuddy;
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using GameTimer;
using FilenameBuddy;

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
			m_Physics = new LevelObjectPhysicsContainer(this);
			States = new ObjectStateContainer(new StateMachine());
			States.StateChangedEvent += this.StateChanged;
		}

		public override void CollisionResponse(IPhysicsContainer rOtherObject,
			CreateAttackAction rAttackAction,
			Vector2 FirstCollisionPoint,
			Vector2 SecondCollisionPoint)
		{
			Debug.Assert(null != m_Physics);
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

		public override bool LoadObject(ContentManager rXmlContent, Filename strResource, IGameDonkey rEngine, int iMessageOffset)
		{
			SPFSettings.LevelObjectXML myCharXML = rXmlContent.Load<SPFSettings.LevelObjectXML>(strResource.GetRelPathFileNoExt());
			if (!base.LoadObject(rXmlContent, myCharXML, rEngine, iMessageOffset))
			{
				return false;
			}

			//set the scale
			Scale = (float)myCharXML.size;

			//set teh position
			Position = myCharXML.location;

			return true;
		}

		#endregion //File IO
	}
}