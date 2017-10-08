using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace GameDonkeyLib
{
	/// <summary>
	/// Manages physics for player objects
	/// </summary>
	public class PlayerPhysicsContainer : BasePhysicsContainer
	{
		#region Methods

		public PlayerPhysicsContainer(BaseObject baseObecjt)
			: base(baseObecjt)
		{
		}

		/// <summary>
		/// check if a push hit occurs between me an another guy, or if we even need to check collisions
		/// </summary>
		/// <param name="otherGuy">the guy to check against</param>
		/// <returns>bool: whether or not I should even check for collisions between these two objects</returns>
		protected override bool CheckPushCollisions(BasePhysicsContainer otherGuy)
		{
			//if either of these dudes are evading, don't bother checking for collisions
			if ((Owner.EvasionTimer.RemainingTime > 0.0f) || (otherGuy.Owner.EvasionTimer.RemainingTime > 0.0f))
			{
				return false;
			}

			//get a vector from that object to this object
			var objVect = Owner.Position - otherGuy.Owner.Position;

			//get the length of the vector between the two dudes
			var currentDistance = objVect.LengthSquared();

			//get the max distance between characters, where we don't even need to check collisions anymore
			var maxDistance = Owner.MaxDistance();
			maxDistance += otherGuy.Owner.MaxDistance();
			if (currentDistance >= (maxDistance * maxDistance))
			{
				//don't bother checking if they are really far away
				return false;
			}

			//get the min distance between characters
			var minDistance = Owner.MinDistance();
			minDistance += otherGuy.Owner.MinDistance();

			//if the length sqrd is too short, we should be moved apart
			if (currentDistance < (minDistance * minDistance))
			{
				if (objVect.LengthSquared() == 0.0f)
				{
					objVect.X = 10.0f;
				}

				//Check if anyone is stunned
				var meStunned = EState.Stunned == (EState)Owner.States.CurrentState;
				var himStunned = EState.Stunned == (EState)otherGuy.Owner.States.CurrentState;

				//get the strength of the hit
				var collisionDelta = (minDistance - objVect.Length());

				if (!meStunned && !himStunned)
				{
					//divide by half, because will be push hit for each charcter
					collisionDelta *= 0.5f; 
				}

				//push this dude?
				if (!meStunned)
				{
					//is this the biggest push hit so far?
					if (!HitFlags[(int)EHitType.PushHit] || (collisionDelta > Hits[(int)EHitType.PushHit].Strength))
					{
						//ok, push this dude away from that other dude

						//get the direction of the hit
						objVect.Normalize();

						//add to my list of hits
						HitFlags[(int)EHitType.PushHit] = true;
						Hits[(int)EHitType.PushHit].Set(objVect, null, collisionDelta, EHitType.PushHit, otherGuy.Owner, Owner.Position);
					}
				}

				//push hit the other dude?
				if (!himStunned)
				{
					if (!otherGuy.HitFlags[(int)EHitType.PushHit] || (collisionDelta > otherGuy.Hits[(int)EHitType.PushHit].Strength))
					{
						//ok, push this dude away from that other dude

						//get the direction of the hit
						objVect.X *= -1.0f;
						objVect.Normalize();

						//add to my list of hits
						otherGuy.HitFlags[(int)EHitType.PushHit] = true;
						otherGuy.Hits[(int)EHitType.PushHit].Set(objVect, null, collisionDelta, EHitType.PushHit, Owner, otherGuy.Owner.Position);
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Recursive function to check if the owner is hitting a level object
		/// </summary>
		/// <param name="levelObject">the level object to check against</param>
		protected override void IterateLevelCollisions(BasePhysicsContainer levelObject)
		{
			//For player objects, only check feet against level objects

			//go through all my foot bones and check for level collision
			for (var i = 0; i < Feet.Count; i++)
			{
				//check this bone for collision
				CheckLevelCollision(Feet[i], levelObject);
			}
		}

		#endregion //Methods
	}
}