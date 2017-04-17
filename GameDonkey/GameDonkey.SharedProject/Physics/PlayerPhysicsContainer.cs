using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace GameDonkeyLib
{
	/// <summary>
	/// Manages physics for player objects
	/// </summary>
	public class PlayerPhysicsContainer : IPhysicsContainer
	{
		#region Methods

		public PlayerPhysicsContainer(BaseObject rObject)
			: base(rObject)
		{
			Debug.Assert((Owner.Type == EObjectType.Human) || (Owner.Type == EObjectType.AI));
		}

		/// <summary>
		/// check if a push hit occurs between me an another guy, or if we even need to check collisions
		/// </summary>
		/// <param name="rOtherGuy">the guy to check against</param>
		/// <returns>bool: whether or not I should even check for collisions between these two objects</returns>
		protected override bool CheckPushCollisions(IPhysicsContainer rOtherGuy)
		{
			//make sure the oither guy is the correct type
			Debug.Assert((Owner.Type == EObjectType.Human) || (Owner.Type == EObjectType.AI));

			//if either of these dudes are evading, don't bother checking for collisions
			if ((Owner.EvasionTimer.RemainingTime() > 0.0f) || (rOtherGuy.Owner.EvasionTimer.RemainingTime() > 0.0f))
			{
				return false;
			}

			//get a vector from that object to this object
			Vector2 objVect = Owner.Position - rOtherGuy.Owner.Position;

			//get the length of the vector between the two dudes
			float fCurDistance = objVect.LengthSquared();

			//get the max distance between characters, where we don't even need to check collisions anymore
			float iMaxDistance = Owner.MaxDistance();
			iMaxDistance += rOtherGuy.Owner.MaxDistance();
			if (fCurDistance >= (iMaxDistance * iMaxDistance))
			{
				//don't bother checking if they are really far away
				return false;
			}

			//get the min distance between characters
			float iMinDistance = Owner.MinDistance();
			iMinDistance += rOtherGuy.Owner.MinDistance();

			//if the length sqrd is too short, we should be moved apart
			if (fCurDistance < (iMinDistance * iMinDistance))
			{
				if (objVect.LengthSquared() == 0.0f)
				{
					objVect.X = 10.0f;
				}

				//Check if anyone is stunned
				bool bMeStunned = EState.Stunned == (EState)Owner.States.CurrentState();
				bool bHimStunned = EState.Stunned == (EState)rOtherGuy.Owner.States.CurrentState();

				//get the strength of the hit
				float fCollisionDelta = (iMinDistance - objVect.Length());

				if (!bMeStunned && !bHimStunned)
				{
					//divide by half, because will be push hit for each charcter
					fCollisionDelta *= 0.5f; 
				}

				//push this dude?
				if (!bMeStunned)
				{
					//is this the biggest push hit so far?
					if (!m_rgHitFlags[(int)EHitType.PushHit] || (fCollisionDelta > m_rgHits[(int)EHitType.PushHit].Strength))
					{
						//ok, push this dude away from that other dude

						//get the direction of the hit
						objVect.Normalize();

						//add to my list of hits
						m_rgHitFlags[(int)EHitType.PushHit] = true;
						m_rgHits[(int)EHitType.PushHit].Set(objVect, null, fCollisionDelta, EHitType.PushHit, rOtherGuy.Owner, Owner.Position);
					}
				}

				//push hit the other dude?
				if (!bHimStunned)
				{
					if (!rOtherGuy.HitFlags[(int)EHitType.PushHit] || (fCollisionDelta > rOtherGuy.Hits[(int)EHitType.PushHit].Strength))
					{
						//ok, push this dude away from that other dude

						//get the direction of the hit
						objVect.X *= -1.0f;
						objVect.Normalize();

						//add to my list of hits
						rOtherGuy.HitFlags[(int)EHitType.PushHit] = true;
						rOtherGuy.Hits[(int)EHitType.PushHit].Set(objVect, null, fCollisionDelta, EHitType.PushHit, Owner, rOtherGuy.Owner.Position);
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Recursive function to check if the owner is hitting a level object
		/// </summary>
		/// <param name="rLevelObject">the level object to check against</param>
		protected override void IterateLevelCollisions(IPhysicsContainer rLevelObject)
		{
			Debug.Assert(EObjectType.Level == rLevelObject.Owner.Type);

			//For player objects, only check feet against level objects

			//go through all my foot bones and check for level collision
			for (int i = 0; i < Feet.Count; i++)
			{
				Debug.Assert(Feet[i].IsFoot);
			
				//check this bone for collision
				CheckLevelCollision(Feet[i], rLevelObject);
			}
		}

		#endregion //Methods
	}
}