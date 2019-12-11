using AnimationLib;
using System.Diagnostics;

namespace GameDonkeyLib
{
	/// <summary>
	/// physics container specific to projectiles
	/// </summary>
	class ProjectilePhysicsContainer : BasePhysicsContainer
	{
		#region Methods

		public ProjectilePhysicsContainer(ProjectileObject projectileObject) : base(projectileObject)
		{
		}

		public override void SortBones(Bone bone)
		{
			if (bone.HasPhysicsData())
			{
				Weapons.Add(bone);

				CollisionBones.Add(bone);
			}

			//run through all the child bones
			for (int i = 0; i < bone.Bones.Count; i++)
			{
				SortBones(bone.Bones[i]);
			}

			BonesSorted = true;
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
			//check the all bones for collision
			for (var i = 0; i < CollisionBones.Count; i++)
			{
				CheckLevelCollision(CollisionBones[i], levelObject);
			}
		}

		#endregion //Methods
	}
}