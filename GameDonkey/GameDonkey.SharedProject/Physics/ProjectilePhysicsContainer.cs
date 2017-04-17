using System.Diagnostics;

namespace GameDonkeyLib
{
	/// <summary>
	/// physics container specific to projectiles
	/// </summary>
	class ProjectilePhysicsContainer : IPhysicsContainer
	{
		#region Methods

		public ProjectilePhysicsContainer(ProjectileObject rObject) : base(rObject)
		{
			Debug.Assert(rObject.Type == EObjectType.Projectile);
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
			Debug.Assert(EObjectType.Level == rLevelObject.Owner.Type);

			//check the all bones for collision
			for (int i = 0; i < CollisionBones.Count; i++)
			{
				CheckLevelCollision(CollisionBones[i], rLevelObject);
			}
		}

		#endregion //Methods
	}
}