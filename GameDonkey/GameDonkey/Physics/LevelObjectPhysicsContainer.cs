using System;

namespace GameDonkeyLib
{
	public class LevelObjectPhysicsContainer : BasePhysicsContainer
	{
		#region Methods

		/// <summary>
		/// Construct one of these objects!
		/// </summary>
		/// <param name="levelObject">the level object that owns this dude</param>
		public LevelObjectPhysicsContainer(LevelObject levelObject) : base(levelObject)
		{
		}

		/// <summary>
		/// Check collisions against another object.
		/// This function NEVER gets called from the level object, level objects are always passed 
		/// INTO this function!
		/// </summary>
		/// <param name="otherGuy"></param>
		public override void CheckCollisions(BasePhysicsContainer otherGuy)
		{
			throw new Exception("don't check level objects like this");
		}

		/// <summary>
		/// check if a push hit occurs between me an another guy, or if we even need to check collisions
		/// </summary>
		/// <param name="otherGuy">the guy to check against</param>
		/// <returns>bool: whether or not I should even check for collisions between these two objects</returns>
		protected override bool CheckPushCollisions(BasePhysicsContainer otherGuy)
		{
			return true;
		}

		/// <summary>
		/// Recursive function to check if the owner is hitting a level object
		/// </summary>
		/// <param name="levelObject">the level object to check against</param>
		protected override void IterateLevelCollisions(BasePhysicsContainer levelObject)
		{
			throw new Exception("should never check level objects against other level objects");
		}

		#endregion //Methods
	}
}