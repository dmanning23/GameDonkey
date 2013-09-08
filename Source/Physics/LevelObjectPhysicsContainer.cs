using System.Diagnostics;

namespace GameDonkey
{
	class CLevelObjectPhysicsContainer : IPhysicsContainer
	{
		#region Methods

		/// <summary>
		/// Construct one of these objects!
		/// </summary>
		/// <param name="rObject">the level object that owns this dude</param>
		public CLevelObjectPhysicsContainer(LevelObject rObject) : base(rObject)
		{
			Debug.Assert(rObject.Type == EObjectType.Level);
		}

		/// <summary>
		/// Check collisions against another object.
		/// This function NEVER gets called from the level object, level objects are always passed 
		/// INTO this function!
		/// </summary>
		/// <param name="rOtherGuy"></param>
		public override void CheckCollisions(IPhysicsContainer rOtherGuy)
		{
			Debug.Assert(false);
		}

		/// <summary>
		/// check if a push hit occurs between me an another guy, or if we even need to check collisions
		/// </summary>
		/// <param name="rOtherGuy">the guy to check against</param>
		/// <returns>bool: whether or not I should even check for collisions between these two objects</returns>
		protected override bool CheckPushCollisions(IPhysicsContainer rOtherGuy)
		{
			return true;
		}

		/// <summary>
		/// Recursive function to check if the owner is hitting a level object
		/// </summary>
		/// <param name="rLevelObject">the level object to check against</param>
		protected override void IterateLevelCollisions(IPhysicsContainer rLevelObject)
		{
			//should never check level objects against other level objects
			Debug.Assert(false);
		}

		#endregion //Methods
	}
}